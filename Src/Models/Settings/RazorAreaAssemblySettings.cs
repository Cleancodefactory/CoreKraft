using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Models.Settings
{
    public class RazorAreaAssemblySettings
    {
        public RazorAreaAssemblySettings()
        {
            RouteMappings = new List<RouteMapping>();
            AssemblyNamesCode = new List<string>();
            AssemblyNamesView = new List<string>();
        }

        public bool IsEnabled { get; set; } = true;

        public bool IsConfigured
        {
            get
            {
                if (AssemblyNamesCode.Count > 0)
                {
                    return !string.IsNullOrEmpty(AssemblyNamesCode[0]) && !string.IsNullOrEmpty(DefaultRouting);
                }
                return false;
            }
        }
        public List<string> AssemblyNamesCode { get; set; }
        public List<string> AssemblyNamesView { get; set; }
        public string DefaultRouting { get; set; }
        public List<RouteMapping> RouteMappings { get; set; }
        public string CatchAllRouting { get; set; }

        public RazorCatchAll ParseRazorCatchAll()
        {
            RazorCatchAll razorCatchAll = new RazorCatchAll();
            if (!string.IsNullOrEmpty(CatchAllRouting))
            {
                string pattern = @"^\{controller=(.*)\}\/\{action=(.*?)\}(?:\/\{(.*?)[\?]?\})?";
                RegexOptions options = RegexOptions.Singleline;

                Match match = Regex.Match(CatchAllRouting, pattern, options);

                if (match.Success)
                {
                    razorCatchAll.CatchAllAction = match.Groups[2].Value;
                    razorCatchAll.CatchAllController = match.Groups[1].Value;
                    razorCatchAll.CatchAllParameter = match.Groups[3].Success ? match.Groups[3].Value : "id";
                    return razorCatchAll;
                }
            }
            return null;
        }
    }

    public class RazorCatchAll
    {
        public string CatchAllAction { get; set; }
        public string CatchAllController { get; set; }
        public string CatchAllParameter { get; set; }
    }
}
