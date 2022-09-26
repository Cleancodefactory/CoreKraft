using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class AuthorizationSection
    {
        public AuthorizationSection()
        {
            Roles = new List<string>();
        }

        public string RedirectAfterLogin { get; set; }
        public bool RequireAuthorization { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<string> Roles { get; set; }
        public List<BuiltinUser> BuiltinUsers { get; set; }
    }
}
