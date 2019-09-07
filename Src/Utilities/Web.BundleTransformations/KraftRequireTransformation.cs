using Ccf.Ck.Utilities.Web.BundleTransformations.Primitives;
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
        private static Regex _FileUsingRegex = new Regex(FILEUSINGPATTERN, RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public string[] Process(KraftBundle kraftBundle, ILogger logger)
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
            KraftBundleProfiles kraftBundleProfiles = kraftBundle as KraftBundleProfiles;
            if (kraftBundleProfiles == null)
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

            //start parsing from the script file of the builder object
            foreach (string profile in kraftBundleProfiles.ProfileFiles)
            {
                FileInfo bootstrapFile = new FileInfo(Path.Combine(kraftBundle.StartDirPath, profile));
                ParseFilesForIncludesRecursive(bootstrapFile, scrRootDir, logger, outputFilesList);
                //outputFilesList.Add(bootstrapFile.ToString());
            }
            outputFilesList = ProcessMappedFiles2VirtualPaths(outputFilesList, kraftBundle.ContentRootPath);
            return outputFilesList.ToArray();
        }

        private List<string> ProcessMappedFiles2VirtualPaths(List<string> outputFilesList, string contentRootPath)
        {
            List<string> virtualFilePaths = new List<string>();
            int pos;
            string fileVirtualName = string.Empty;
            foreach (string file in outputFilesList)
            {
                pos = file.IndexOf(contentRootPath);
                if (pos < 1)
                {
                    fileVirtualName = file.Remove(pos, contentRootPath.Length);
                }
                virtualFilePaths.Add(fileVirtualName.Replace(@"\", @"/"));
            }
            return virtualFilePaths;
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
