using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager.Http
{
    internal class Loader
    {
        internal static async Task<T> LoadAsync<T>(CancellationToken cancellationToken, AuthenticationHeaderValue authHeader, HttpMethod method, Dictionary<string, string> parameters, string url)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                //specify to use TLS 1.3 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                client.DefaultRequestHeaders.Authorization = authHeader;
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    if (method == HttpMethod.Post)
                    {
                        request.Content = new FormUrlEncodedContent(parameters);
                    }
                    
                    using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        Stream stream = await response.Content.ReadAsStreamAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            return DeserializeJsonFromStream<T>(stream);
                        }
                        string content = await StreamToStringAsync(stream);
                        throw new ApiException
                        {
                            StatusCode = (int)response.StatusCode,
                            Content = content
                        };
                    }
                }
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default(T);
            }

            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                JsonSerializer js = new JsonSerializer();
                T result = js.Deserialize<T>(jtr);
                return result;
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }

            return content;
        }
    }
}
