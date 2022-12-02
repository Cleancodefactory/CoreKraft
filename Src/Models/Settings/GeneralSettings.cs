using Ccf.Ck.Models.Enumerations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Models.Settings
{
    public class GeneralSettings
    {
        private Dictionary<string, string> _ModuleKey2Path;
        private bool _EnumParsed = false;
        private EMetaInfoFlags _MetaInfoFlag;

        public GeneralSettings()
        {
            AuthorizationSection = new AuthorizationSection();
            SignalRSettings = new SignalRSettings();
            RazorAreaAssembly = new RazorAreaAssemblySettings();
            SupportedLanguages = new List<string>();
            Theme = "Module";
            ToolsSettings = new ToolsSettings();
            WatchSubFoldersForRestart = new List<string>();
            SignalSettings = new SignalSettings();
            SpaSettings = new SpaSettings();
            HistoryNavSettings = new HistoryNavSettings();
            HostingServiceSettings = new List<HostingServiceSetting>();
        }
        public string ServerHostKey { get; set; }
        public bool EnableOptimization { get; set; }
        public List<string> ModulesRootFolders { get; set; }
        public string DefaultStartModule { get; set; }
        public string PageTitle { get; set; }
        public string PassThroughJsConfig { get; set; }
        public string MetaLogging { get; set; }
        public EMetaInfoFlags MetaLoggingEnumFlag
        {
            get
            {
                if (_EnumParsed)
                {
                    return _MetaInfoFlag;
                }
                _EnumParsed = true;
                if (!Enum.TryParse(MetaLogging, out _MetaInfoFlag))
                {
                    _MetaInfoFlag = EMetaInfoFlags.None;
                }
                
                return _MetaInfoFlag;                
            }
        }
        public string BindKraftConfiguration
        {
            get;
            private set;
        }
        public string Theme { get; set; }
        public string KraftUrlSegment { get; set; }
        public string KraftUrlCssJsSegment { get; set; }
        public string KraftUrlResourceSegment { get; set; }
        public string KraftUrlModuleImages { get; set; }
        public string KraftUrlModulePublic { get; set; }
        public string KraftRequestFlagsKey { get; set; }
        public string HostingUrl { get; set; }
        public AuthorizationSection AuthorizationSection { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool RedirectToHttps { get; set; }
        public bool RedirectToWww { get; set; }
        public SignalRSettings SignalRSettings { get; set; }
        public SignalSettings SignalSettings { get; set; }
        public SpaSettings SpaSettings { get; set; }
        public HistoryNavSettings HistoryNavSettings { get; set; }
        public List<HostingServiceSetting> HostingServiceSettings { get; set; }
        public List<string> MetaTags { get; set; }
        public ProgressiveWebAppSettings ProgressiveWebApp { get; set; }
        public RazorAreaAssemblySettings RazorAreaAssembly { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public List<string> WatchSubFoldersForRestart { get; set; }

        public ToolsSettings ToolsSettings { get; set; }

        public void ReplaceMacrosWithPaths(string contentRootPath, string wwwRootPath)
        {
            Regex regex = new Regex(@"(?:(@(?<first>wwwroot|contentroot)@))|(?:%(?<env>[a-zA-Z0-9_]+)%*)");
            for (int i = 0; i < ModulesRootFolders.Count; i++)
            {
                ModulesRootFolders[i] = regex.Replace(ModulesRootFolders[i], m =>
                {
                    if (m.Groups["first"].Success)//wwwroot|contentroot
                    {
                        switch (m.Groups["first"].Value)
                        {
                            case "wwwroot":
                                {
                                    return wwwRootPath;
                                }
                            case "contentroot":
                                {
                                    return contentRootPath;
                                }
                            default:
                                break;
                        }
                    }
                    else if (m.Groups["env"].Success)//%something%
                    {
                        string envVariable = Environment.GetEnvironmentVariable(m.Groups["env"].Value);
                        if (!string.IsNullOrEmpty(envVariable))
                        {
                            return envVariable;
                        }
                        else
                        {
                            return string.Empty; //Variable not valid or not populated
                        }
                    }
                    return null;
                });
            }
            ModulesRootFolders = ModulesRootFolders.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            DirectoryInfo directoryInfo;
            foreach (string module in ModulesRootFolders)
            {
                directoryInfo = new DirectoryInfo(module);
                if (!directoryInfo.Exists)
                {
                    throw new Exception($"Configured path for start module: {module} is not valid and doesn't exist!");
                }
            }
        }

        public bool GetBindKraftConfigurationContent(IWebHostEnvironment webHostEnvironment)
        {
            if (PassThroughJsConfig != null)
            {
                FileInfo configFile = new FileInfo(Path.Combine(webHostEnvironment.ContentRootPath, PassThroughJsConfig));
                if (configFile.Exists)
                {
                    var blockComments = @"/\*(.*?)\*/";
                    var lineComments = @"//(.*?)\r?\n";
                    var strings = @"""((\\[^\n]|[^""\n])*)""";
                    var verbatimStrings = @"@(""[^""]*"")+";
                    BindKraftConfiguration = Regex.Replace(File.ReadAllText(configFile.FullName, Encoding.UTF8),
                    blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                    me =>
                    {
                        if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                            return me.Value.StartsWith("//") ? Environment.NewLine : "";
                        // Keep the literal strings
                        return me.Value;
                    },
                    RegexOptions.Singleline);
                    //Restart application if file changes
                    return true;
                }
            }

            BindKraftConfiguration = "{}";

            return false;
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

        public bool EnableThemeChange { get; set; }
    }
}
