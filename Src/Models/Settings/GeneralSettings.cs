namespace Ccf.Ck.Models.Settings
{
    public class GeneralSettings
    {
        public GeneralSettings()
        {
            AuthorizationSection = new AuthorizationSection();
            SignalRSettings = new SignalRSettings();
        }
        public bool EnableOptimization { get; set; }
        public string ModulesRootFolder { get; set; }
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
            ModulesRootFolder = ModulesRootFolder.Replace("@wwwroot@", wwwRootPath).Replace("@contentroot@", contentRootPath);
        }
    }
}
