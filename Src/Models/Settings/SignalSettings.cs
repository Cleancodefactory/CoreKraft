using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SignalSettings
    {
        public SignalSettings()
        {
            OnSystemStartup = new List<string>();
            OnSystemShutdown = new List<string>();
        }
        public List<string> OnSystemStartup { get; set; }
        public List<string> OnSystemShutdown { get; set; }
    }
}