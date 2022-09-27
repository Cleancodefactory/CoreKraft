using Ccf.Ck.Models.NodeSet;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.Interfaces
{

    public interface ISecurityModel
    {
        bool IsBuiltin { 
            get {
                return false;
            }
        }
        bool IsAuthenticated { get; }

        string UserName { get; }

        string UserEmail { get; }

        string FirstName { get; }

        string LastName { get; }

        ICollection<string> Roles { get; }

        int IsInRole(string roleName) {
            var rolex = Roles.FirstOrDefault(r => string.CompareOrdinal(r, roleName) == 0);
            if (rolex != null) return 0;
            return 1;
        }
        /// <summary>
        /// Change this if we add more Security props.
        /// // {Security}
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool CheckSecurity(Security sec) {
            if (sec == null) return true; // No security
            if (sec.BuiltinOnly) {
                if (!IsBuiltin) return false;
            }
            if (sec.AllowRoles != null) {
                var roles = sec.AllowRoles.Intersect(Roles);
                if (roles != null && roles.Count() > 0) return true;
                return false;
            } else {
                return true; // No role list - let everybody in
            } 
        }
    }
}
