using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Ccf.Ck.Models.NodeSet {
    public class Security {
        public Security() {

        }
        public Security(bool? requireAuthentication = false) {
            RequireAuthentication = requireAuthentication.HasValue ? requireAuthentication.Value : false;
        }
        public Security(Security security, bool requireAuthentication = false) {
            if (security != null) {
                BuiltinOnly = security.BuiltinOnly;
                AllowRoles = security?.AllowRoles?.ToArray(); // Copy the roles
                RequireAuthentication = requireAuthentication;
            }
        }
        public static bool AnyTrue(params bool?[] bools) {
            return bools.Any(b => b.HasValue && b.Value);
        }
        public static Security From(Security security, bool? requireAuthentication = false) {
            if (security != null) {
                return new Security(security, requireAuthentication.HasValue ? requireAuthentication.Value : false);
            }
            return new Security(requireAuthentication);
        }
        public static Security From(NodeSet nodeset) {
            if (nodeset == null) return From(null, null);    
            return From(nodeset.Security, AnyTrue(nodeset?.Security?.RequireAuthentication,nodeset.RequireAuthentication));
        }
        public static Security From(Node node) {
            if (node == null) return From(null, null);
            return From(node.Security, AnyTrue(node?.Security?.RequireAuthentication,node.RequireAuthentication));
        }
        public static Security From(OperationBase readWrite) {
            if (readWrite == null) return From(null, null);
            return From(readWrite.Security);
        }

        public bool RequireAuthentication { get; set; } = false;
        public bool BuiltinOnly { get; set; } = false;
        public ICollection<string> AllowRoles { get; set; } = null;

        /// <summary>
        /// Helper for following the override chain
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public Security OverrideWith(Security security, bool requireAuthentication = false) {
            if (security != null) {
                if (security.BuiltinOnly) {
                    BuiltinOnly = security.BuiltinOnly;
                }
                if (security.AllowRoles != null) {
                    AllowRoles = security.AllowRoles.ToArray();
                }
                if (!RequireAuthentication) {
                    RequireAuthentication = security.RequireAuthentication;
                    if (requireAuthentication) RequireAuthentication = true;
                }
            }
            return this;
        }
        public Security OverrideWith(NodeSet nodeset) {
            if (nodeset == null) return this;
            return OverrideWith(nodeset.Security, nodeset.RequireAuthentication);
        }
        public Security OverrideWith(Node node) {
            if (node == null) return this;
            return OverrideWith(node.Security, node.RequireAuthentication);
        }
        public Security OverrideWith(OperationBase node) {
            if (node == null) return this;
            return OverrideWith(node.Security);
        }
    }
}