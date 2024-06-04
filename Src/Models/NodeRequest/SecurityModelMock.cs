using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.NodeRequest
{
    public class SecurityModelMock : ISecurityModel
    {
        private readonly AuthorizationSection _AuthorizationSection;

        public SecurityModelMock(AuthorizationSection authorizationSection)
        {
            _AuthorizationSection = authorizationSection;
        }
        public bool IsAuthenticated => true;

        public bool IsBuiltin => true;

        public string UserName => _AuthorizationSection.UserName;

        public string UserEmail => _AuthorizationSection.UserEmail;

        public string FirstName => _AuthorizationSection.FirstName;

        public string LastName => _AuthorizationSection.LastName;

        public ICollection<string> Roles => _AuthorizationSection.Roles;

        public int IsInRole(string roleName)
        {
            if (IsAuthenticated)
            {
                ICollection<string> roles = Roles;
                if (roles != null && roles.Count > 0)
                {
                    string role = roles.FirstOrDefault(c => c.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(role))
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
    }
}
