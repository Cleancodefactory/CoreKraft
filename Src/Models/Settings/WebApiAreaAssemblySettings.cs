using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Models.Settings
{
    public class WebApiAreaAssemblySettings
    {
        public WebApiAreaAssemblySettings()
        {
            RouteMappings = new List<RouteMapping>();
            AssemblyNamesCode = new List<string>();
            AssemblyNamesView = new List<string>();
        }
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
    }
}
