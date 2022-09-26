using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.Interfaces
{

    public interface ISecurityModel
    {
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
    }
}
