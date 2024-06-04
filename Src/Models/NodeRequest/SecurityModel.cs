using Ccf.Ck.Models.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public string NameIdentifier
        {
            get
            {
                if (IsAuthenticated)
                {
                    return _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                }
                return null;
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
                    string email = _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                    if (string.IsNullOrEmpty(email))
                    {
                        email = _ClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    }
                    return email;
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
                    ICollection<string> roles = _ClaimsPrincipal.Claims.Where(c => c.Type == "role").Select(s => s.Value).ToList();
                    if (roles == null || roles?.Count == 0)
                    {
                        roles = _ClaimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(s => s.Value).ToList();
                    }
                    return roles;
                }
                return null;
            }
        }

        public bool IsBuiltin
        {
            get
            {
                return false;
            }
        }

        public int IsInRole(string roleName)
        {
            if (IsAuthenticated)
            {
                ICollection<string> roles = Roles;
                if (roles != null && roles.Count> 0)
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
