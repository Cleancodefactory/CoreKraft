using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Data.FileUpload.BaseClasses;
using Ccf.Ck.SysPlugins.Data.FileUpload.Models;
using Ccf.Ck.SysPlugins.Data.FileUpload.Validator;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.FileUpload
{
    public class FileUploadImp : DataLoaderClassicBase<FileUploadSynchronizeContextScopedImp>, IDisposable
    {
        private const string NAME = "name";
        private const string ORIGINALFILENAME = "originalfilename";
        private const string CONTENTTYPE = "contenttype";
        private const string FILENAME = "filename";
        private const string LENGTH = "length";
        private const string UPLOADEDFILES = "uploadedfiles";
        private const string ISNULLORWHITESPACE = "{0} cannot be null, empty or white space.";

        private const string UPLOADFOLDER = "UploadFolder";
        private const string DEFAULTFOLDER = "DefaultFolder";
        private const string MAXFILESPACEPERDASHBOARDINMB = "MaxFilesSpacePerDashboardInMB";
        private const string PREVIEWHEIGHT = "PreviewHeight";
        private const string PREVIEWWIDTHT = "PreviewWidth";
        private const string FILENOTFOUNDICON = "FileNotFoundIcon";

        private ImageValidator _ImageValidator;

        private bool isDisposed = false;

        public FileUploadImp() : base()
        {
            _ImageValidator = new ImageValidator();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (_ImageValidator != null)
                {
                    _ImageValidator.Dispose();
                    _ImageValidator = null;
                }

                isDisposed = !isDisposed;
            }
        }

        /// <summary>
        /// Reads uploaded file in predefined directory,
        /// sets the response builder of the return model of the processing context to BinaryResponseBuilder.
        /// </summary>
        /// <param name="execContext">Execution context</param>
        /// <returns>null</returns>
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            ReadCustomSettings settings = new ReadCustomSettings(execContext, execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings)));

            string filePreviewName = "FILE_preview.svg";
            string previewName = "preview";
            string file = execContext.Evaluate("path").Value.ToString();
            string configuredDir = settings.UploadFolder;
            string defaultDir = settings.DefaultFolder;

            if (File.Exists(Path.Combine(configuredDir, file)))
            {
                ParameterResolverValue preview = execContext.Evaluate(previewName);

                if (preview.Value != null)
                {
                    string previewValue = preview.Value.ToString();

                    if (previewValue == "1")
                    {
                        string previewFileName = GeneratePreviewName(file);

                        if (File.Exists(Path.Combine(configuredDir, previewFileName)))
                        {
                            file = previewFileName;
                        }
                        else
                        {
                            string extension = Path.GetExtension(file);

                            if (!string.IsNullOrWhiteSpace(extension))
                            {
                                previewFileName = extension.Replace(".", string.Empty).ToUpper() + "_preview.svg";

                                if (File.Exists(Path.Combine(defaultDir, previewFileName)))
                                {
                                    file = previewFileName;
                                    configuredDir = defaultDir;
                                }
                                else
                                {
                                    file = filePreviewName;
                                    configuredDir = defaultDir;
                                }
                            }
                            else
                            {
                                file = filePreviewName;
                                configuredDir = defaultDir;
                            }
                        }
                    }
                }
            }
            else
            {
                if (File.Exists(Path.Combine(defaultDir, settings.FileNotFoundIcon)))
                {
                    configuredDir = defaultDir;
                    file = settings.FileNotFoundIcon;
                }
                else
                {
                    KraftLogger.LogError($"FileUploadImp-Read: File with name {file} was not found.");
                }
            }

            file = Path.Combine(configuredDir, file);
            string contentType;

            new FileExtensionContentTypeProvider().TryGetContentType(file, out contentType);

            execContext.ProcessingContext.ReturnModel.BinaryData = new PostedFile(contentType, 0, "currentlyNotSet", file, path =>
            {
                return File.Open(path as string, FileMode.Open);
            }, file);

            execContext.ProcessingContext.ReturnModel.ResponseBuilder = new BinaryResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { execContext.ProcessingContext }));

            return null;
        }

        /// <summary>
        /// Uploads or deletes new file to predefined directory
        /// </summary>
        /// <param name="execContext">Execution context</param>
        /// <returns>null</returns>
        protected override object Write(IDataLoaderWriteContext execContext)
        {
            //Get the configured directory
            WriteCustomSettings settings = new WriteCustomSettings(execContext, execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings)));

            if (execContext.Operation == OPERATION_DELETE)
            {
                string fileName = execContext.Evaluate("name").Value.ToString();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new Exception("File name cannot be null, empty or white spaced.");
                }

                return DeleteFile(settings.UploadFolder, fileName);
            }
            else if (execContext.Operation == OPERATION_INSERT)
            {
                return UploadFiles(execContext, settings);
            }

            return null;
        }

        /// <summary>
        /// Deletes file from predefined directory, 
        /// it will delete also the preview file (if such exists).
        /// </summary>
        /// <param name="dir">Predefined directory</param>
        /// <param name="filename">The name of the file</param>
        /// <returns>null</returns>
        private object DeleteFile(string dir, string filename)
        {
            string file = Path.Combine(dir, filename);

            if (File.Exists(file))
            {
                DeleteFile(file);

                string filepreview = Path.Combine(dir, GeneratePreviewName(filename));

                if (File.Exists(filepreview))
                {
                    DeleteFile(filepreview);
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes a file with given path.
        /// </summary>
        /// <param name="path">The path of the file</param>
        private void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"File {path} can't be deleted.", ex);
            }
        }

        /// <summary>
        /// Uploads a file to predefined directory.
        /// </summary>
        /// <param name="execContext">Execution context</param>
        /// <param name="settings">Custom defined settings in conficuration file.</param>
        /// <returns>Dictionary of string objects containing the information of the uploaded files.</returns>
        private object UploadFiles(IDataLoaderWriteContext execContext, WriteCustomSettings settings)
        {
            CheckMaxFileSpacePerDashboard(execContext, settings.FileRestrictions);
            CreateDirectoryIfSuchDoesNotExist(settings.UploadFolder);

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            double height = settings.PreviewHeight ?? 60D;
            double width = settings.PreviewWidth ?? 120D;
            string fileName;
            string previewName;

            HtmlEncoder encoder = HtmlEncoder.Default;

            foreach (object item in execContext.ProcessingContext.InputModel.Data.Values)
            {
                if (item is IPostedFile postedFile)
                {
                    fileName = RenameUploadedFileIfNameExists(postedFile.FileName, settings.UploadFolder);

                    if (postedFile.ContentType.IndexOf("image", StringComparison.InvariantCultureIgnoreCase) > -1 && _ImageValidator.GetValidMimeTypes.Any(s => postedFile.ContentType.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1 && _ImageValidator.ValidateIfStreamIsImage(postedFile.OpenReadStream(), s)))
                    {
                        using (IImageEditor imageEditor = new ImageEditor(postedFile))
                        {
                            try
                            {
                                imageEditor.Save(Path.Combine(settings.UploadFolder, fileName));

                                previewName = GeneratePreviewName(fileName);

                                imageEditor.Resize(width, height);
                                imageEditor.Save(Path.Combine(settings.UploadFolder, previewName));
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        using (var stream = new FileStream(Path.Combine(settings.UploadFolder, fileName), FileMode.Create))
                        {
                            postedFile.OpenReadStream().CopyTo(stream);
                        }
                    }

                    result.Add(new Dictionary<string, object>()
                    {
                        { NAME, string.IsNullOrEmpty(postedFile.Name) ? postedFile.FileName : encoder.Encode(postedFile.Name) },//user input please escape
                        { FILENAME, fileName },
                        { CONTENTTYPE, postedFile.ContentType },
                        { ORIGINALFILENAME, postedFile.FileName },
                        { LENGTH, postedFile.Length }
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Check the maximum allowed file size per custom settings configuration if any.
        /// </summary>
        /// <param name="execContext">Execution context</param>
        /// <param name="fileRestrictions">Nullable long</param>
        private void CheckMaxFileSpacePerDashboard(IDataLoaderWriteContext execContext, Dictionary<string, long> fileRestrictions)
        {
            long bytes = 1048576L;

            if (fileRestrictions != null)
            {
                foreach (string key in fileRestrictions.Keys)
                {
                    ParameterResolverValue resolverValue = execContext.Evaluate(key);
                    long uploadedFilesSize = 0L;

                    if (resolverValue.Value != null)
                    {
                        if (!long.TryParse(execContext.Evaluate(key).Value.ToString(), out uploadedFilesSize))
                        {
                            throw new Exception($"Invalid configuration settings for key {key}.");
                        }
                    }

                    long maxAllowedSize = bytes * (long)fileRestrictions[key];

                    if (maxAllowedSize < uploadedFilesSize + execContext.ProcessingContext.InputModel.Data.Values.Where(v => v is IPostedFile).Cast<IPostedFile>().Sum(pf => pf.Length))
                    {
                        throw new Exception($"You cannot upload more than {fileRestrictions[key]} MB.");
                    }
                }
            }
        }

        /// <summary>
        /// If provided directory does not exists it is created.
        /// </summary>
        /// <param name="directory">Provided directory.</param>
        private void CreateDirectoryIfSuchDoesNotExist(string directory)
        {
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating directory - {directory}.", ex);
                }
            }
        }

        /// <summary>
        /// Renames file name if the preddefined directory already contains another file with the same file name.
        /// </summary>
        /// <param name="fileName">The newly uploaded file.</param>
        /// <param name="configuredDir">Predefined directory.</param>
        /// <returns>The new name of the file.</returns>
        private string RenameUploadedFileIfNameExists(string fileName, string configuredDir)
        {
            int postfix = 0;
            string underscore = "_";
            string testName = fileName;
            string name = testName;

            while (File.Exists(Path.Combine(configuredDir, name)))
            {
                name = $"{Path.GetFileNameWithoutExtension(testName)}{underscore}{(postfix++).ToString()}{Path.GetExtension(fileName)}";
            }

            return name;
        }

        /// <summary>
        /// Generates preview file name based on given name.
        /// </summary>
        /// <param name="fileName">The file name from which will be generated the preview name.</param>
        /// <returns></returns>
        private string GeneratePreviewName(string fileName)
        {
            return $"{Path.GetFileNameWithoutExtension(fileName)}_preview{Path.GetExtension(fileName)}";
        }

        /// <summary>
        /// Custom settings for read operation.
        /// </summary>
        private class ReadCustomSettings
        {
            private const string EXCEPTIONMESSAGE = "The custom setting {0} was not found in configuration settings.";
            private const string MODULEROOT = "@moduleroot@";
            private const string MODULE = "@module@";

            internal ReadCustomSettings(IDataLoaderContext execContext, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
            {
                if (!execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(UPLOADFOLDER, out string uploadFolder))
                {
                    throw new Exception(string.Format(EXCEPTIONMESSAGE, UPLOADFOLDER));
                }

                UploadFolder = uploadFolder.Replace(MODULEROOT, kraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(execContext.ProcessingContext.InputModel.Module)).Replace(MODULE, execContext.ProcessingContext.InputModel.Module);

                if (!execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(DEFAULTFOLDER, out string defaultFolder))
                {
                    throw new Exception(string.Format(EXCEPTIONMESSAGE, DEFAULTFOLDER));
                }

                DefaultFolder = defaultFolder.Replace(MODULEROOT, kraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(execContext.ProcessingContext.InputModel.Module)).Replace(MODULE, execContext.ProcessingContext.InputModel.Module); ;

                if (!execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(FILENOTFOUNDICON, out string fileNotFoundIcon))
                {
                    throw new Exception(string.Format(EXCEPTIONMESSAGE, FILENOTFOUNDICON));
                }

                FileNotFoundIcon = fileNotFoundIcon;
            }

            internal string UploadFolder { get; private set; }

            internal string DefaultFolder { get; private set; }

            internal string FileNotFoundIcon { get; private set; }
        }

        /// <summary>
        /// Custom settings for write operation.
        /// </summary>
        private class WriteCustomSettings : ReadCustomSettings
        {
            internal WriteCustomSettings(IDataLoaderContext execContext, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(execContext, kraftGlobalConfigurationSettings)
            {
                Dictionary<string, long> fileRestrictions = new Dictionary<string, long>();
                double? previewHeight = null;
                double? previewWidth = null;

                if (execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(MAXFILESPACEPERDASHBOARDINMB, out string parameters))
                {
                    long value;
                    Regex keyValueRegex = new Regex(@"{(\s*(?<key>[a-zA-Z]+)\s*:\s*(?<value>[0-9]+)\s*)}");
                    MatchCollection matchCollection = keyValueRegex.Matches(parameters);

                    foreach (Match match in matchCollection)
                    {
                        if (long.TryParse(match.Groups["value"].ToString(), out value))
                        {
                            if (value < 0L)
                            {
                                throw new Exception("Invalid configuration for uploading files. Restriction cannot be less than zero.");
                            }

                            fileRestrictions.Add(match.Groups["key"].ToString(), value);
                        }
                        else
                        {
                            throw new Exception("Invalid configuration for uploading files.");
                        }
                    }
                }

                FileRestrictions = fileRestrictions;

                if (execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(PREVIEWHEIGHT, out string previewHeightAsString))
                {
                    previewHeight = ParseDouble(previewHeightAsString);
                }

                PreviewHeight = previewHeight;

                if (execContext.DataLoaderContextScoped.CustomSettings.TryGetValue(PREVIEWWIDTHT, out string previewWidthAsString))
                {
                    previewWidth = ParseDouble(previewWidthAsString);
                }

                PreviewWidth = previewWidth;
            }

            internal Dictionary<string, long> FileRestrictions { get; private set; }

            internal double? PreviewHeight { get; private set; }

            internal double? PreviewWidth { get; private set; }

            private double? ParseDouble(string valueAsString)
            {
                double? result = null;

                if (double.TryParse(valueAsString, out double value))
                {
                    if (result <= 0D)
                    {
                        throw new Exception("Preview size cannot be less or equal to zero.");
                    }

                    result = value;
                }

                return result;
            }
        }
    }
}