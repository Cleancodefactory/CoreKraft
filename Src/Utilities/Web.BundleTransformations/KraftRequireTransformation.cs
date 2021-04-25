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

        public string[] Process(KraftBundle kraftBundle, IApplicationBuilder app, ILogger logger)
        {
            #region Check validity
            if (kraftBundle == null)
            {
                throw new ArgumentNullException(nameof(kraftBundle));
            }
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
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

            //start parsing from the script file of the builder object
            foreach (string profile in kraftBundleProfiles.ProfileFiles)
            {
                FileInfo bootstrapFile = new FileInfo(Path.Combine(kraftBundle.StartDirPath, profile));
                if (bootstrapFile.Exists)//Check the profile files one by one and break if first found
                {
                    ParseFilesForIncludesRecursive(bootstrapFile, scrRootDir, logger, outputFilesList);
                    break;
                }
                //outputFilesList.Add(bootstrapFile.ToString());
            }
            outputFilesList = ProcessMappedFiles2VirtualPaths(outputFilesList, kraftBundle.ContentRootPath, app);
            return outputFilesList.ToArray();
        }

        private List<string> ProcessMappedFiles2VirtualPaths(List<string> outputFilesList, string contentRootPath, IApplicationBuilder app)
        {
            List<string> virtualFilePaths = new List<string>();
            int pos;
            string fileVirtualName = string.Empty;
            foreach (string file in outputFilesList)
            {
                pos = file.IndexOf(contentRootPath);
                if (pos == 0)
                {
                    fileVirtualName = file.Remove(pos, contentRootPath.Length);
                }
                else if (pos == -1) //Content root not found, obviously an optional dependency outside of the content root
                {
                    //RegisterStaticFiles(app, file, "modules", "css", "images");

                }
                virtualFilePaths.Add(fileVirtualName.Replace(@"\", @"/"));
            }
            return virtualFilePaths;
        }

        internal static void RegisterStaticFiles(IApplicationBuilder builder, string modulePath, string startNode, string resourceSegmentName, string type)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(modulePath);
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(modulePath, type));
            if (dirInfo.Exists)
            {
                builder.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = false,
                    DefaultContentType = "image/png",
                    FileProvider = new PhysicalFileProvider(dirInfo.FullName),
                    RequestPath = new PathString($"/{startNode}/{resourceSegmentName}/{directoryInfo.Name}/{type}"),
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    }
                });
            }
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
