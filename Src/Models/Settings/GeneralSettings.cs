using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.Models.Settings
{
    public class GeneralSettings
    {
        private Dictionary<string, string> _ModuleKey2Path;

        public GeneralSettings()
        {
            AuthorizationSection = new AuthorizationSection();
            SignalRSettings = new SignalRSettings();
            Theme = "Module";
        }
        public bool EnableOptimization { get; set; }
        public List<string> ModulesRootFolders { get; set; }
        public string DefaultStartModule { get; set; }
        public string Theme { get; set; }
        public string KraftUrlSegment { get; set; }
        public string KraftUrlCssJsSegment { get; set; }
        public string KraftUrlResourceSegment { get; set; }
        public string KraftUrlModuleImages { get; set; }
        public string KraftUrlModulePublic { get; set; }
        public string KraftRequestFlagsKey { get; set; }
        public AuthorizationSection AuthorizationSection { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool RedirectToHttps { get; set; }
        public bool RedirectToWww { get; set; }
        public SignalRSettings SignalRSettings { get; set; }
        public SignalSettings SignalSettings { get; set; }
        public HostingServiceSettings HostingServiceSettings { get; set; }

        public void ReplaceMacrosWithPaths(string contentRootPath, string wwwRootPath)
        {
            string path;
            for (int i = 0; i < ModulesRootFolders.Count; i++)
            {
                path = ModulesRootFolders[i].Replace("@wwwroot@", wwwRootPath).Replace("@contentroot@", contentRootPath);
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    throw new Exception($"Configured path: {path} is not valid and doesn't exist!");
                }
                ModulesRootFolders[i] = directoryInfo.FullName;
            }
        }

        public string ModulesRootFolder(string moduleKey)
        {
            return _ModuleKey2Path[moduleKey.ToLower()];
        }

        public Dictionary<string, string> ModuleKey2Path
        {
            set
            {
                _ModuleKey2Path = value;
            }
        }
    }
}
