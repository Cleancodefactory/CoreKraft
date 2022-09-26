using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class Security {
        public Security() { }
        public Security(Security security) {
            if (security != null) {
                BuiltinOnly = security.BuiltinOnly;
                AllowRoles = security?.AllowRoles.ToArray(); // Copy the roles
            }
        }
        public static Security From(Security security) {
            if (security != null) {
                return new Security(security);
            }
            return null;
        }

        public bool BuiltinOnly { get; set; }
        public ICollection<string> AllowRoles { get; set; }

        /// <summary>
        /// Helper for following the override chain
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public Security OverrideWith(Security security) {
            if (security != null) {
                if (security.BuiltinOnly) {
                    BuiltinOnly = security.BuiltinOnly;
                }
                if (security.AllowRoles != null) {
                    AllowRoles = security.AllowRoles.ToArray();
                }
            }
            return this;
        }

    }
}
