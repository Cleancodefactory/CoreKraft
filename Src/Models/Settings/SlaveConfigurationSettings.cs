using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SlaveConfigurationSettings
    {
        public SlaveConfigurationSettings()
        {
            Sections = new List<string>();
        }

        public List<string> Sections { get; set; }
    }
}