using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class AuthorizationSection
    {
        public AuthorizationSection()
        {
            Roles = new List<string>();
        }
        public bool RequireAuthorization { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<string> Roles { get; set; }
    }
}
