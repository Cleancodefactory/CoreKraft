<<<<<<< HEAD
﻿using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;

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
            foreach (string signal in _KraftGlobalConfigurationSettings.GeneralSettings?.SignalSettings?.OnSystemStartup ?? new List<string>())
            {
                ExecuteSignals("null", signal);
            }
        }
    }
}
=======
﻿using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;

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
            foreach (string signal in _KraftGlobalConfigurationSettings.GeneralSettings?.SignalSettings?.OnSystemStartup ?? new List<string>())
            {
                ExecuteSignals("null", signal);
            }
        }
    }
}
>>>>>>> develop
