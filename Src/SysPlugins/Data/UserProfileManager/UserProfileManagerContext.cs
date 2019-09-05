using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager
{
    public class UserProfileManagerContext : IPluginsSynchronizeContextScoped
    {
        public UserProfileManagerContext()
        {
            this.CustomSettings = new Dictionary<string, string>();
        }

        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
