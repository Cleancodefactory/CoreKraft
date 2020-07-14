using Ccf.Ck.Models.Web.Settings;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class KraftGlobalConfigurationSettings //: IConfigurationModel
    {
        public GeneralSettings GeneralSettings { get; set; }
        public List<OverrideModuleSetting> OverrideModuleSettings { get; set; }
        public KraftEnvironmentSettings EnvironmentSettings { get; set; }

        public void Reset()
        {
            GeneralSettings = new GeneralSettings();
            OverrideModuleSettings = new List<OverrideModuleSetting>();
        }

        public Dictionary<string, string> GetOverrideCustomSettings(string moduleName, string loaderName)
        {
            foreach (OverrideModuleSetting moduleSettings in OverrideModuleSettings)
            {
                if (moduleSettings.Name != null && moduleSettings.Name.Equals(moduleName, System.StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Loader loader in moduleSettings.Loaders)
                    {
                        if (loader.LoaderName != null && loader.LoaderName.Equals(loaderName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return loader.CustomSettings;
                        }
                    }
                }
            }
            return null;
        }
    }
}
