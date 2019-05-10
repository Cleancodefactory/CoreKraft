using Ccf.Ck.Models.Settings;
using System;

namespace Ccf.Ck.Web.Middleware
{
    internal class SignalStartup : SignalBase
    {
        internal SignalStartup(IServiceProvider serviceProvider, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _ServiceProvider = serviceProvider;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }
        internal void ExecuteSignalsOnStartup()
        {
            foreach (string signal in _KraftGlobalConfigurationSettings.GeneralSettings?.SignalSettings?.OnSystemStartup)
            {
                ExecuteSignals("null", signal);
            }
        }
    }
}
