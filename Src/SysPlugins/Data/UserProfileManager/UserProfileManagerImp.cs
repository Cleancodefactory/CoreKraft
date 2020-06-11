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

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager
{
    public class UserProfileManagerImp : DataLoaderClassicBase<UserProfileManagerContext>
    {       
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (!string.IsNullOrEmpty(execContext.CurrentNode.Read.Select?.Query) && execContext.CurrentNode.Read.Select.Query.Equals("bearer", System.StringComparison.OrdinalIgnoreCase))
            {
                string accessToken = GetAuthAccessToken(execContext);
                KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings = execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                string authority = kraftGlobalConfigurationSettings.GeneralSettings.Authority;
                authority = authority.TrimEnd('/');
                Dictionary<string, object> resultAuth = new Dictionary<string, object> ();
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
            //else
            //{
            //    CancellationToken cancellationToken = new CancellationToken();
            //    string targetUrl = GetAuthUrl(execContext, "select");
            //    AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", accessToken);
            //    Task<Dictionary<string, object>> resultAuth =
            //        Loader.LoadAsync<Dictionary<string, object>>(cancellationToken, authenticationHeaderValue, HttpMethod.Get, null, targetUrl);
            //    resultAuth.ConfigureAwait(false);
            //    result.Add(resultAuth.Result);
            //}

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

        //protected override object Write(IDataLoaderWriteContext execContext)
        //{            
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    parameters = MapParameters(execContext.Row);
        //    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        //    CancellationToken cancellationToken = new CancellationToken();
        //    string targetUrl = GetAuthUrl(execContext, execContext.Operation);
        //    AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", GetAuthAccessToken(execContext));
        //    Task<Dictionary<string, object>> resultAuth = Loader.LoadAsync<Dictionary<string, object>>(cancellationToken, authenticationHeaderValue, HttpMethod.Post, parameters, targetUrl);
        //    resultAuth.ConfigureAwait(false);
        //    result.Add(resultAuth.Result);
        //    return result;
        //}

        //private Dictionary<string, string> MapParameters(Dictionary<string, object> row)
        //{
        //    Dictionary<string, string> result = new Dictionary<string, string>();
        //    string given_name = row["given_name"] == null ? "null" : row["given_name"].ToString();
        //    string family_name = row["family_name"] == null ? "null" : row["family_name"].ToString();
        //    string email = row["email"] == null ? "null" : row["email"].ToString();
        //    string phone_number = row["phone_number"] == null ? "null" : row["phone_number"].ToString();
        //    string roles = row["roles"].ToString();
        //    string communication_consent = row["communication_consent"] == null ? "false" : row["communication_consent"].ToString();

        //    result.Add("given_name", given_name);
        //    result.Add("family_name", family_name);
        //    result.Add("email", email);
        //    result.Add("phone_number", phone_number);
        //    result.Add("roles", roles);
        //    result.Add("communication_consent", communication_consent);

        //    return result;
        //}

        //private string GetAuthUrl(IDataLoaderContext execContext, string action)
        //{
        //    KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings = execContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
        //    string authority = kraftGlobalConfigurationSettings.GeneralSettings.Authority;
        //    authority = authority.TrimEnd('/');
        //    switch (action) //select, insert, update, delete
        //    {
        //        case "select":
        //            {
        //                return $"{authority}/api/getuser";
        //            }
        //        case "update":
        //            {
        //                return $"{authority}/api/updateuser";
        //            }
        //        case "delete":
        //            {
        //                return $"{authority}/api/deleteuser";
        //            }
        //        case "avatar":
        //            {
        //                return $"{authority}/api/avatar";
        //            }
        //        case "authority":
        //            {
        //                return $"{authority}";
        //            }
        //    }
        //    return null;
        //}


    }
}
