using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeRequest {
    public class SecurityModelBuiltIn : ISecurityModel {
        public SecurityModelBuiltIn() {
        }
        public SecurityModelBuiltIn(string user, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            LoadBuiltin(user, kraftGlobalConfigurationSettings); // TODO Do we prefer eception to be thrown
        }


        public static SecurityModelBuiltIn Create(string user, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            var sm = new SecurityModelBuiltIn();
            if (sm.LoadBuiltin(user, kraftGlobalConfigurationSettings)) return sm;
            return null;
        }
        private bool LoadBuiltin(string user, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            var builtins = kraftGlobalConfigurationSettings.GeneralSettings?.AuthorizationSection?.BuiltinUsers;
            if (builtins == null) return false;
            var usr = builtins.FirstOrDefault(u => string.CompareOrdinal(u.UserName,user) == 0);
            if (usr != null) {
                IsBuiltin = true;
                IsAuthenticated = true;
                UserName = usr.UserName;
                UserEmail = usr.UserEmail;
                FirstName = usr.FirstName;
                LastName = usr.LastName;
                Roles = usr.Roles.ToArray();
                return true;
            }
            return false;
        }

        public bool IsBuiltin { get; private set; } = false;

        public bool IsAuthenticated { get; private set; } = false;

        public string UserName { get; private set; } = null;

        public string UserEmail { get; private set; } = null;

        public string FirstName { get; private set; } = null;

        public string LastName { get; private set; } = null;

        public ICollection<string> Roles {get; private set;} = null;
        
    }
}
