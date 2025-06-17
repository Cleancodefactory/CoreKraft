using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using System;
using System.IO;
using System.Threading;

namespace Ccf.Ck.Utilities.GlobalAccessor
{
    public class FileOperation : IFileOperation
    {
        /// <summary>
        /// Relative path to the root of the application
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Relative path to the root of the application
        /// </summary>
        public string SourcePath { get; set; }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                throw new InvalidOperationException("SourcePath cannot be null or empty.");
            }

            string basePath = GlobalAccessor.Instance.GetKraftGlobalConfigurationSettings().EnvironmentSettings.WebRootPath ?? AppContext.BaseDirectory;

            string sourceFullPath = Path.GetFullPath(Path.Combine(basePath, SourcePath));
            string targetFullPath = string.IsNullOrWhiteSpace(TargetPath)
                ? null
                : Path.GetFullPath(Path.Combine(basePath, TargetPath));

            if (!File.Exists(sourceFullPath))
            {
                throw new FileNotFoundException("Source file not found.", sourceFullPath);
            }

            //throw GlobalAccessorException 

            try
            {
                if (targetFullPath == null)
                {
                    File.Delete(sourceFullPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFullPath)!);
                    File.Copy(sourceFullPath, targetFullPath, overwrite: true);
                    File.Delete(sourceFullPath);
                }
            }
            catch (IOException ex)
            {
                KraftLogger.LogError(ex, $"Error during file operation from {sourceFullPath} to {targetFullPath}.");
                if (IsFileLocked(sourceFullPath))
                {
                    throw new GlobalAccessorException($"File: {targetFullPath} is locked. Retry later.");
                }                
            }
        }

        private static bool IsFileLocked(string path)
        {
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public string GetKey()
        {
            return $"{this.SourcePath}|{this.TargetPath}";
        }
    }
}
