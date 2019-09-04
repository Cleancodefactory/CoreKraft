using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Data.FileTransaction;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.Reflection;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.SysPlugins.Data.RawFiles
{
    public class RawFileDataImp : DataLoaderClassicBase<FileDataSynchronizeContextScopedImp>
    {
        protected override object Write(IDataLoaderWriteContext execContext)
        {
            //Node node = (Node)execContext.CurrentNode;
            //string directoryPath = Path.Combine(
            //execContext.ProcessingContext.InputModel.EnvironmentSettings.ContentRootPath,
            //"Modules",
            //execContext.ProcessingContext.InputModel.Module,
            //"Public");

            //var filename = execContext.Evaluate("rawfilename");
            //var filecontents = execContext.Evaluate("rawfilecontents");
            //string fileName = filename.Value.ToString();
            //string filePath = Path.Combine(directoryPath, fileName);
            //string fileContents = filecontents.Value.ToString();

            //FileSystemTransaction fst = ((ITransactionScope)execContext.OwnContextScoped).StartTransaction() as FileSystemTransaction;

            ////Check if you need something else from the InputModel.Data
            //if(!File.Exists(filePath)) fst.Create(filePath);
            //fst.Write(filePath, fileContents);

            //return new Dictionary<string, object>(); /*{
            //    { "filename", fileName},
            //    { "content", fileContents },
            //    { "length", fileContents.Length}
            //};*/

            KraftGlobalConfigurationSettings kraftSettings =
                execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));

            string modulePath = string.Empty;

            execContext.DataLoaderContextScoped.CustomSettings.TryGetValue("Path", out modulePath);

            if (kraftSettings != default(KraftGlobalConfigurationSettings) || modulePath.Length == 0)
            {
                string path = Path.Combine(
                    kraftSettings.GeneralSettings.ModulesRootFolder(execContext.ProcessingContext.InputModel.Module),
                    execContext.ProcessingContext.InputModel.Module,
                    modulePath);

                if (Directory.Exists(path))
                {
                    string[] commands;

                    switch (execContext.Operation)
                    {
                        case "insert":
                            commands = execContext.CurrentNode.Write.Insert.Query.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                            break;
                        case "update":
                            commands = execContext.CurrentNode.Write.Update.Query.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                            break;
                        case "delete":
                            commands = execContext.CurrentNode.Write.Delete.Query.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                            break;
                        default:
                            throw new Exception(string.Empty);
                    }

                    if (commands == default(string[]) || commands.Length < 0)
                    {
                        throw new Exception(string.Empty);
                    }

                    string currentCommand = string.Empty;
                    FileSystemTransaction fst = ((ITransactionScope)execContext.OwnContextScoped).StartTransaction() as FileSystemTransaction;
                    Type fileSystemCommandType = typeof(IFileSystemCommand);

                    foreach (string commandName in commands)
                    {
                        currentCommand = commandName + "Command";

                        Type type = AppDomain.CurrentDomain.GetAssemblies()
                            ?.FirstOrDefault(a => a.GetTypes().Any(t => fileSystemCommandType.IsAssignableFrom(t)))
                            ?.GetTypes()
                            ?.FirstOrDefault(t => String.Equals(currentCommand, t.Name, StringComparison.OrdinalIgnoreCase) && fileSystemCommandType.IsAssignableFrom(t)) ?? throw new Exception(string.Empty);

                        ConstructorInfo constructorInfo = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

                        ParameterInfo[] parameters = constructorInfo.GetParameters();
                        string[] parameterValues = new string[parameters.Length];
                        string value = string.Empty;

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            value = execContext.Evaluate(parameters[i].Name.ToLower()).Value?.ToString() ?? throw new Exception(string.Empty);
                            parameterValues[i] = Path.Combine(path, value);
                        }

                        IFileSystemCommand command = (IFileSystemCommand)Activator.CreateInstance(type, parameterValues);

                        fst.Add(command);
                    }

                    if (fst.HasCommands())
                    {
                        fst.Commit();
                    }
                }
            }

            return null;

            //throw new Exception(string.Empty);
        }

        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            Node node = (Node)execContext.CurrentNode;
            string directoryPath = Path.Combine(
            execContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(execContext.ProcessingContext.InputModel.Module),
            execContext.ProcessingContext.InputModel.Module,
            "Public");

            var filename = execContext.Evaluate("rawfilename");

            if(filename.Value == null)
            {
                PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
                return new List<Dictionary<string, object>>() {
                    new Dictionary<string, object>() {
                        { "files", Directory.GetFiles(directoryPath)}
                    }
                };
            }

            string fileName = filename.Value.ToString();
            string filePath = Path.Combine(directoryPath, fileName);
            string cacheKey = $"{filePath}_Raw";
            ICachingService cachingService = execContext.PluginServiceManager.GetService<ICachingService>(typeof(ICachingService));
            string cachedRaw = cachingService.Get<string>(cacheKey);
            if (cachedRaw == null)
            {
                PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
                try
                {
                    cachedRaw = File.ReadAllText(filePath);
                } 
                catch (Exception ex)
                {
                    execContext.ProcessingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                    throw;
                }
                cachingService.Insert(cacheKey, cachedRaw, fileProvider.Watch(fileName));
            }
            return new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "filename", fileName},
                    { "content", cachedRaw },
                    { "length", cachedRaw.Length}
                }
            };
        }

        //public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        //{
        //    return await Task.FromResult<IPluginsSynchronizeContextScoped>(new FileDataSynchronizeContextScopedImp());
        //}

        private ILogger GetLogger(IPluginServiceManager pluginServiceManager)
        {
            ILogger logger = pluginServiceManager.GetService<ILogger>(typeof(ILogger));
            logger.LogDebug("Hi from raw file reader");
            return logger;
        }
    }
}
