using Ccf.Ck.Models.NodeSet;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.Interfaces
{
    public interface ISecurityModel
    {
        bool IsBuiltin { get; }

        bool IsAuthenticated { get; }

        string UserName { get; }

        string UserEmail { get; }

        string FirstName { get; }

        string LastName { get; }

        ICollection<string> Roles { get; }

        int IsInRole(string roleName) {
            var rolex = Roles.FirstOrDefault(r => string.CompareOrdinal(r, roleName) == 0);
            if (rolex != null) return 1;
            return 0;
        }
        /// <summary>
        /// Change this if we add more Security props.
        /// // {Security}
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool CheckSecurity(Security sec) {
            if (sec == null) return true; // No security
            if (sec.RequireAuthentication && !IsAuthenticated) return false;
            if (sec.BuiltinOnly) {
                if (!IsBuiltin || !IsAuthenticated) return false;
            }
            if (sec.AllowRoles != null) {
                if (!IsAuthenticated) return false;
                var roles = sec.AllowRoles.Intersect(Roles);
                if (roles != null && roles.Count() > 0) return true;
                return false;
            } else {
                return true; // No role list - let everybody in
            } 
        }
        /// <summary>
        /// Returns true if passing through authentication can solve the issue
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool NeedsAuthentication(Security sec) {
            if (sec == null) return false; // No security
            if (sec.BuiltinOnly) {
                return false;
            }
            if (sec.RequireAuthentication && !IsAuthenticated) return true;
            if (sec.AllowRoles != null) {
                if (!IsAuthenticated) return true;
            } 
            return false;
            
        }
    }
}
