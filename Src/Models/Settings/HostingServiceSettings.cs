using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class HostingServiceSetting
    {
        public List<string> Signals { get; set; }
        public int IntervalInMinutes { get; set; }
    }
}
