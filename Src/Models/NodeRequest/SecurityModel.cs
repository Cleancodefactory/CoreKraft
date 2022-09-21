using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Ccf.Ck.Models.Enumerations;

namespace Ccf.Ck.Models.NodeRequest
{
    public class SecurityModel : ISecurityModel
    {
        private readonly ClaimsPrincipal _ClaimsPrincipal;
        public SecurityModel(HttpContext httpContext)
        {
            IAuthenticationService authenticationService = httpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            Task<AuthenticateResult> authResult = authenticationService.AuthenticateAsync(httpContext, OpenIdConnectDefaults.AuthenticationScheme);
            _ClaimsPrincipal = authResult.Result.Principal;
        }
        public bool IsAuthenticated
        {
            get
            {
                return _ClaimsPrincipal?.Identity?.IsAuthenticated ?? false;
            }
        }
        public string UserName
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                }
                return null;
            }
        }

        public string UserEmail
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                }
                return null;
            }
        }

        public string FirstName
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "firstname")?.Value;
                }
                return null;
            }
        }

        public string LastName
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "lastname")?.Value;
                }
                return null;
            }
        }

        public ICollection<string> Roles
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.Where(c => c.Type == "role").Select(s => s.Value).ToList();
                }
                return null;
            }
        }

        public int IsInRole(string roleName)
        {
            if (IsAuthenticated)
            {
                Claim claim = _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "role" && c.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (claim != null)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}
