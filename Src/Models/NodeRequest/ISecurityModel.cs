using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeRequest
{
    public interface ISecurityModel
    {
        bool IsAuthenticated { get; }

        string UserName { get; }

        string UserEmail { get; }

        string FirstName { get; }

        string LastName { get; }

        ICollection<string> Roles { get; }

        int IsInRole(string roleName);
    }
}
