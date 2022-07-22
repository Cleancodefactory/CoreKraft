using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.Json;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using System.Text.Json;

namespace Ccf.Ck.SysPlugins.Data.Json
{
    public class JsonDataImp : DataLoaderClassicBase<JsonDataSynchronizeContextScopedImp> // IDataLoaderPlugin
    {

        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            Node node = (Node)execContext.CurrentNode;
            Dictionary<string, object> currentResult = null;
            JsonDataSynchronizeContextScopedImp jsonDataSynchronizeContextScopedImp = execContext.OwnContextScoped as JsonDataSynchronizeContextScopedImp;

            if (jsonDataSynchronizeContextScopedImp == null)
            {
                throw new NullReferenceException("The contextScoped is not JsonSyncronizeContextScopedImp");
            }

            /*
            string directoryPath = contextScoped.CustomSettings[START_FOLDER_PATH_JSON_DATA]
                    .Replace("@wwwroot@", processingContext.InputModel.EnvironmentSettings.WebRootPath)
                    .Replace("@treenodename@", loaderContext.NodeSet.Name); 
             */

            string directoryPath = Path.Combine(
                        execContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(execContext.ProcessingContext.InputModel.Module),
                        execContext.ProcessingContext.InputModel.Module,
                        "Data");

            if (node?.Read?.Select?.HasFile() == true)
            {
                string filePath = Path.Combine(directoryPath, node.Read.Select.File);

                string cacheKey = $"{execContext.ProcessingContext.InputModel.Module}{execContext.ProcessingContext.InputModel.NodeSet}{execContext.ProcessingContext.InputModel.Nodepath}{node.NodeKey}_Json";
                ICachingService cachingService = execContext.PluginServiceManager.GetService<ICachingService>(typeof(ICachingService));
                string cachedJson = cachingService.Get<string>(cacheKey);
                if (cachedJson == null)
                {
                    PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
                    try
                    {
                        cachedJson = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        execContext.ProcessingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                        throw;
                    }

                    cachingService.Insert(cacheKey, cachedJson, fileProvider.Watch(node.Read.Select.File));
                }
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                options.Converters.Add(new DictionaryStringObjectJsonConverter());
                currentResult = new Dictionary<string, object>(System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(cachedJson, options));

            }
            return new List<Dictionary<string, object>>() { currentResult };
        }

        protected override object Write(IDataLoaderWriteContext execContext)
        {
            //throw new NotImplementedException();
            return null;
        }

        //public async Task<object> ExecuteAsync(IDataLoaderContext execContext)
        //{

        //    ILogger logger = GetLogger(execContext.PluginServiceManager);
        //    Node node = (Node)execContext.CurrentNode;
        //    ReadOnlyDictionary<string, object> currentResult = null;
        //    JsonDataSynchronizeContextScopedImp jsonDataSynchronizeContextScopedImp = execContext.OwnContextScoped as JsonDataSynchronizeContextScopedImp;

        //    if (jsonDataSynchronizeContextScopedImp == null)
        //    {
        //        throw new NullReferenceException("The contextScoped is not JsonSyncronizeContextScopedImp");
        //    }

        //    /*
        //    string directoryPath = contextScoped.CustomSettings[START_FOLDER_PATH_JSON_DATA]
        //            .Replace("@wwwroot@", processingContext.InputModel.EnvironmentSettings.WebRootPath)
        //            .Replace("@treenodename@", loaderContext.NodeSet.Name); 
        //     */

        //    string directoryPath = Path.Combine(
        //                execContext.ProcessingContext.InputModel.EnvironmentSettings.ContentRootPath,
        //                "Modules",
        //                execContext.ProcessingContext.InputModel.Module,
        //                "Data");

        //    if (node?.Read?.Select?.HasFile == true)
        //    {
        //        string filePath = Path.Combine(directoryPath, node.Read.Select.File);

        //        string cacheKey = $"{execContext.ProcessingContext.InputModel.NodeSet}{execContext.ProcessingContext.InputModel.Nodepath}{node.NodeKey}_Json";
        //        ICachingService cachingService = execContext.PluginServiceManager.GetService<ICachingService>(typeof(ICachingService));
        //        string cachedJson = cachingService.Get<string>(cacheKey);
        //        if (cachedJson == null)
        //        {
        //            PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryPath);
        //            try
        //            {
        //                cachedJson = File.ReadAllText(filePath);
        //            }
        //            catch (Exception ex)
        //            {
        //                execContext.ProcessingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
        //                throw;
        //            }

        //            cachingService.Insert(cacheKey, cachedJson, fileProvider.Watch(node.Read.Select.File));
        //        }
        //        currentResult = new ReadOnlyDictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedJson, new DictionaryConverter()));              
        //    }
        //    else if (node?.Write?.Update?.HasFile == true && execContext.ProcessingContext?.InputModel?.IsWriteOperation == true)
        //    {
        //        //TODO: 
        //        FileSystemTransaction fst = ((ITransactionScope)execContext.OwnContextScoped).StartTransaction() as FileSystemTransaction;
        //        string pathTemp = Path.Combine(directoryPath, node.Write.Update.File);
        //        //Check if you need something else from the InputModel.Data
        //        string content = JsonConvert.SerializeObject(execContext.ProcessingContext.InputModel.Data["data"]);
        //        fst.Write(pathTemp, "{\"data\":" + content + "}");
        //        currentResult = execContext.ProcessingContext.InputModel.Data;
        //        //cachingService.Insert(cacheKey, content, fileProvider.Watch(fullFilePath));
        //    }

        //    return await Task.FromResult(currentResult);
        //}

        //public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        //{
        //    return await Task.FromResult<IPluginsSynchronizeContextScoped>(new JsonDataSynchronizeContextScopedImp());
        //}



        private ILogger GetLogger(IPluginServiceManager pluginServiceManager)
        {
            ILogger logger = pluginServiceManager.GetService<ILogger>(typeof(ILogger));
            logger.LogDebug("Hi from json reader");
            return logger;
        }
    }
}
