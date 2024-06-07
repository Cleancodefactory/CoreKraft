using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class WebApiAreaAssemblySettings
    {
        public WebApiAreaAssemblySettings()
        {
            AssemblyNames = new List<string>();
        }
        public bool IsConfigured
        {
            get
            {
                if (AssemblyNames.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public List<string> AssemblyNames { get; set; }
        public bool EnableSwagger { get; set; }
    }
}
