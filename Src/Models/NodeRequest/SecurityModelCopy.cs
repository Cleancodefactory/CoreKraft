using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeRequest {
    public class SecurityModelCopy :ISecurityModel {
        public SecurityModelCopy(ISecurityModel model) {
            if (model != null) {
                IsAuthenticated = model.IsAuthenticated;
                UserName = model.UserName;
                UserEmail = model.UserEmail;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Roles = model.Roles.ToList();
            }
        }

        public bool IsAuthenticated { get; private set; } = false;

        public string UserName { get; private set; } = null;

        public string UserEmail { get; private set; } = null;

        public string FirstName { get; private set; } = null;

        public string LastName { get; private set; } = null;

        public ICollection<string> Roles {get; private set;} = null;
        
    }
}
