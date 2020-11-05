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
        }
        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrEmpty(AssemblyNameCode) && !string.IsNullOrEmpty(AssemblyNameViews) && !string.IsNullOrEmpty(DefaultRouting);
            }
        }
        public string AssemblyNameCode { get; set; }
        public string AssemblyNameViews { get; set; }
        public string DefaultRouting { get; set; }
        public List<RouteMapping> RouteMappings { get; set; }
    }
}
