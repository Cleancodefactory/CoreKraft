using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ccf.Ck.Libs.Logging;

namespace Ccf.Ck.Web.Middleware
{
    public static class KraftKeepAlive
    {
        private static string _BaseUrl;
        public static void RegisterKeepAliveAsync(IApplicationBuilder app)
        {
            IApplicationLifetime appLifeCycle = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            appLifeCycle?.ApplicationStopping.Register(OnApplicationStopping);
        }

        public static void SetBaseUrl(string baseUrl)
        {
            _BaseUrl = baseUrl;
        }

        private static async void OnApplicationStopping()
        {
            if (string.IsNullOrEmpty(_BaseUrl))
            {
                KraftLogger.LogDebug("Method: OnApplicationStopping: BaseUrl is null.");
                return;
            }
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage responseMessage = await client.GetAsync(_BaseUrl))
                {
                    using (HttpContent content = responseMessage.Content)
                    {
                        KraftLogger.LogDebug($"Method: OnApplicationStopping: Calling the application {_BaseUrl} to keepalive.");
                        await content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
