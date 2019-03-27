using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SignalSettings
    {
        public List<string> OnSystemStartup { get; set; }
        public List<string> OnSystemShutdown { get; set; }
    }
}