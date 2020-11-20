using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.Settings
{
    public class RazorAreaAssemblySettings
    {
        public RazorAreaAssemblySettings()
        {
            RouteMappings = new List<RouteMapping>();
            AssemblyNamesCode = new List<string>();
        }
        public bool IsConfigured
        {
            get
            {
                if (!string.IsNullOrEmpty(AssemblyNameViews) && !string.IsNullOrEmpty(DefaultRouting))
                {
                    if (AssemblyNamesCode.Count > 0)
                    {
                        return !string.IsNullOrEmpty(AssemblyNamesCode[0]);
                    }
                }
                return false;
            }
        }
        public List<string> AssemblyNamesCode { get; set; }
        public string AssemblyNameViews { get; set; }
        public string DefaultRouting { get; set; }
        public List<RouteMapping> RouteMappings { get; set; }
    }
}
