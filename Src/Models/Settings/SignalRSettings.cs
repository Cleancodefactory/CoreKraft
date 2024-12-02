using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SignalRSettings
    {
        public SignalRSettings()
        {
            SignalRHubOptions = new SignalRHubOptions();
            SignalRHttpConnectionOptions = new SignalRHttpConnectionOptions();
        }
        public bool UseSignalR { get; set; }

        public SignalRHubOptions SignalRHubOptions { get; set; }
        public SignalRHttpConnectionOptions SignalRHttpConnectionOptions { get; set; }

        public string HubImplementationAsString { get; set; }

        public string HubRoute { get; set; }

        public Dictionary<string, string> Settings { get; set; }
    }
}
