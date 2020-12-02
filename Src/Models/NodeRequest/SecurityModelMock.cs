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

        public string UserName => _AuthorizationSection.UserName;

        public string UserEmail => _AuthorizationSection.UserEmail;

        public string FirstName => _AuthorizationSection.FirstName;

        public string LastName => _AuthorizationSection.LastName;

        public ICollection<string> Roles => _AuthorizationSection.Roles;

        public int IsInRole(string roleName)
        {
            string role = _AuthorizationSection.Roles.FirstOrDefault(c => c.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            return role != null ? 1 : 0;
        }
    }
}
