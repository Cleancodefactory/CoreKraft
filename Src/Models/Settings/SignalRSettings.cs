using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SignalRSettings
    {
        public bool UseSignalR { get; set; }

        public string HubImplementationAsString { get; set; }

        public string HubRoute { get; set; }

        public Dictionary<string, string> Settings { get; set; }
    }
}
