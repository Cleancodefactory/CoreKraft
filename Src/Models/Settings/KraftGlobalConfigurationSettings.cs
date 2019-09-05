using Ccf.Ck.Models.Web.Settings;

namespace Ccf.Ck.Models.Settings
{
    public class KraftGlobalConfigurationSettings //: IConfigurationModel
    {
        public GeneralSettings GeneralSettings { get; set; }
        public KraftEnvironmentSettings EnvironmentSettings { get; set; }

        public void Reset()
        {
            GeneralSettings = new GeneralSettings();
        }
    }
}
