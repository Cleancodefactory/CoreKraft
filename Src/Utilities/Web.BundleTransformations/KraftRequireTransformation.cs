using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Utilities.Web.BundleTransformations.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Utilities.Web.BundleTransformations
{
    public class KraftRequireTransformation
    {
        private const string FILEUSINGPATTERN = "^((\\s*)|(\\s*\\/\\/){1})#(?i)using(?-i)\\s*\\\"(?<files>.*)\\\"";
        private static readonly Regex _FileUsingRegex = new Regex(FILEUSINGPATTERN, RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public InputFile[] Process(KraftBundle kraftBundle, string modulePath, string moduleName, string rootVirtualPath, ILogger logger)
        {
            #region Check validity
            if (kraftBundle == null)
            {
                throw new ArgumentNullException(nameof(kraftBundle));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (string.IsNullOrEmpty(kraftBundle.ContentRootPath))
            {
                throw new ArgumentNullException(nameof(kraftBundle.ContentRootPath));
            }
            if (string.IsNullOrEmpty(kraftBundle.StartDirPath))
            {
                throw new ArgumentNullException(nameof(kraftBundle.StartDirPath));
            }
            if (!(kraftBundle is KraftBundleProfiles kraftBundleProfiles))
            {
                return null;
            }
            if (kraftBundleProfiles.ProfileFiles == null)
            {
                throw new ArgumentNullException(nameof(kraftBundleProfiles.ProfileFiles));
            }
            #endregion Check validity

            List<string> outputFilesList = new List<string>();
            DirectoryInfo scrRootDir = new DirectoryInfo(kraftBundle.StartDirPath);

            foreach (string profile in kraftBundleProfiles.ProfileFiles)
            {
                FileInfo bootstrapFile = new FileInfo(Path.Combine(kraftBundle.StartDirPath, profile));
                if (bootstrapFile.Exists)//Check the profile files one by one and break if first found
                {
                    ParseFilesForIncludesRecursive(bootstrapFile, scrRootDir, logger, outputFilesList);
                    break;
                }
            }

            InputFile[] inputFiles = ProcessMappedFiles2VirtualPaths(outputFilesList, modulePath, moduleName);
            return inputFiles;
        }

        private InputFile[] ProcessMappedFiles2VirtualPaths(List<string> outputFilesList, string modulePath, string moduleName)
        {
            int pos;
            InputFile[] inputFiles = new InputFile[outputFilesList.Count];
            string file;
            for (int i = 0; i < outputFilesList.Count; i++)
            {
                file = outputFilesList[i];
                pos = file.IndexOf(modulePath);
                if (pos == 0)
                {
                    string fileVirtualName = "/modules/" + moduleName + file.Remove(pos, modulePath.Length);
                    inputFiles[i] = new InputFile { PhysicalPath = file, VirtualPath = fileVirtualName.Replace(@"\", @"/") };
                }
                else
                {
                    throw new Exception($"Can't create virtual path for file {outputFilesList[i]}");
                }
            }
            return inputFiles;
        }

        private void ParseFilesForIncludesRecursive(FileInfo currentFile, DirectoryInfo currentDir, ILogger logger, List<string> outputFilesList)
        {
            if (currentFile.Exists)
            {
                if (currentFile.Extension.Equals(".dep", StringComparison.InvariantCultureIgnoreCase))
                {
                    MatchCollection matches = _FileUsingRegex.Matches(File.ReadAllText(currentFile.FullName));
                    foreach (Match match in matches)
                    {
                        string fileName = match.Groups["files"].Value;
                        FileInfo inclFileFName = new FileInfo(Path.GetFullPath(Path.Combine(currentDir.FullName, fileName)));
                        ParseFilesForIncludesRecursive(inclFileFName, new DirectoryInfo(inclFileFName.DirectoryName), logger, outputFilesList);
                        if (!outputFilesList.Contains(inclFileFName.FullName))
                        {
                            if (!inclFileFName.Extension.Equals(".dep", StringComparison.InvariantCultureIgnoreCase))
                            {
                                outputFilesList.Add(inclFileFName.FullName);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Referenced file: {currentFile.FullName} was not found. Please correct and restart.");
            }
        }
    }
}
