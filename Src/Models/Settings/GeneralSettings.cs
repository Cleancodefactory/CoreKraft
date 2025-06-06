﻿using Ccf.Ck.Models.Enumerations;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
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
            CookieStoreSection = new CookieStoreSection();
            SignalRSettings = new SignalRSettings();
            RazorAreaAssembly = new RazorAreaAssemblySettings();
            BlazorAreaAssembly = new BlazorAreaAssemblySettings();
            SlaveConfiguration = new SlaveConfigurationSettings();
            SupportedLanguages = new List<string>();
            Theme = "Module";
            ToolsSettings = new ToolsSettings();
            WatchSubFoldersForRestart = new List<string>();
            SignalSettings = new SignalSettings();
            SpaSettings = new SpaSettings();
            HistoryNavSettings = new HistoryNavSettings();
            HostingServiceSettings = new List<HostingServiceSetting>();
            WebApiAreaAssembly = new WebApiAreaAssemblySettings();
            MiddleWares = new List<MiddleWareSettings>();
            CorsAllowedOrigins = new CorsAllowedOrigins();
        }
        public string DataStatePropertyName { get; set; } = null; // if missing the default value 'state' is used
        public int TaskThreads { get; set; } = 0; // auto by default
        public int MaxAutoTaskThreads { get; set; } = 4; // has effect only if auto is set
        public string ServerHostKey { get; set; }
        public bool EnableOptimization { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool FieldNameToLowerCase { get; set; } = true; //default is true
        public CorsAllowedOrigins CorsAllowedOrigins { get; set; }
        public List<string> ModulesRootFolders { get; set; }
        public string DefaultStartModule { get; set; }
        public string EnableBufferQueryParameter { get; set; }
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
        public CookieStoreSection CookieStoreSection { get; set; }
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
        public List<MiddleWareSettings> MiddleWares { get; set; }
        public List<string> MetaTags { get; set; }
        public ProgressiveWebAppSettings ProgressiveWebApp { get; set; }
        public RazorAreaAssemblySettings RazorAreaAssembly { get; set; }
        public BlazorAreaAssemblySettings BlazorAreaAssembly { get; set; }
        public WebApiAreaAssemblySettings WebApiAreaAssembly { get; set; }
        public SlaveConfigurationSettings SlaveConfiguration { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public List<string> WatchSubFoldersForRestart { get; set; }

        public ToolsSettings ToolsSettings { get; set; }

        public void ReplaceMacrosWithPaths(string contentRootPath, string wwwRootPath)
        {
            Regex regex = new Regex(@"(?:(@(?<first>wwwroot|contentroot)@))|(?:%(?<env>[a-zA-Z0-9_]+)%*)");
            List<int> _invalids = new List<int>();
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
                        //Console.WriteLine("envval:" + envVariable);
                        if (!string.IsNullOrWhiteSpace(envVariable))
                        {
                            return envVariable;
                        }
                        else
                        {
                            _invalids.Add(i);
                            return string.Empty; //Variable not valid or not populated
                        }
                    }
                    return null;
                });
            }
            //ModulesRootFolders = ModulesRootFolders.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            //Console.WriteLine("invalids:" + _invalids.Count);
            for (int j = _invalids.Count - 1; j >=0 ; j--) {
                ModulesRootFolders.RemoveAt(_invalids[j]);
            }
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
            return _ModuleKey2Path[moduleKey.ToLowerInvariant()];
        }

        public Dictionary<string, string> ModuleKey2Path
        {
            set
            {
                _ModuleKey2Path = value;
            }
        }

        public bool EnableThemeChange { get; set; }
        public bool RemovePropertyState { get; set; } = false;
        public bool DontSetState { get; set; } = false;
    }
}
