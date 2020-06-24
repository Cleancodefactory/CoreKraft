using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Data.UserProfileManager.Http;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Principal;
using System.Security.Claims;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager
{
    public class UserProfileManagerImp : DataLoaderClassicBase<UserProfileManagerContext>
    {       
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (!string.IsNullOrEmpty(execContext.CurrentNode.Read.Select?.Query) && execContext.CurrentNode.Read.Select.Query.Equals("bearer", System.StringComparison.OrdinalIgnoreCase))
            {
                if (execContext.ProcessingContext.InputModel.SecurityModel.IsAuthenticated)//Try to get bearer token only when user is logged in
                {
                    string accessToken = GetAuthAccessToken(execContext);
                    KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings = execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                    string authority = kraftGlobalConfigurationSettings.GeneralSettings.Authority;
                    authority = authority.TrimEnd('/');
                    Dictionary<string, object> resultAuth = new Dictionary<string, object>();
                    resultAuth.Add("key", $"{authority}/api/avatar");
                    resultAuth.Add("token", accessToken);
                    resultAuth.Add("servicename", "avatarimage");
                    result.Add(resultAuth);
                    resultAuth = new Dictionary<string, object>();
                    resultAuth.Add("key", $"{authority}");
                    resultAuth.Add("token", accessToken);
                    resultAuth.Add("servicename", "authorizationserver");
                    result.Add(resultAuth);
                    resultAuth = new Dictionary<string, object>();
                    resultAuth.Add("key", $"{authority}/api/user");
                    resultAuth.Add("token", accessToken);
                    resultAuth.Add("servicename", "user");
                    result.Add(resultAuth);
                }
            }

            return result;
        }

        protected override object Write(IDataLoaderWriteContext execContext)
        {
            throw new System.NotImplementedException();
        }

        private string GetAuthAccessToken(IDataLoaderContext execContext)
        {
            IHttpContextAccessor httpContextAccessor = execContext.PluginServiceManager.GetService<IHttpContextAccessor>(typeof(HttpContextAccessor));

            Task<string> accessTokenTask = httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (accessTokenTask.IsFaulted)//occurs when no authentication is included
            {
                return null;
            }
            return accessTokenTask.Result;
        }
    }
}
