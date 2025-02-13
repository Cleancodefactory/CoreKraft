using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager.Http
{
    internal class Loader
    {
        internal static async Task<T> LoadAsync<T>(CancellationToken cancellationToken, AuthenticationHeaderValue authHeader, HttpMethod method, 
            Dictionary<string, string> parameters, string url)
        {
            // Create a handler configured to use TLS 1.2 and TLS 1.3
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = authHeader;

            using var request = new HttpRequestMessage(method, url);
            if (method == HttpMethod.Post)
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Read the response stream.
            using var stream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
            {
                return DeserializeJsonFromStream<T>(stream);
            }

            // Read the error content and throw an exception.
            string content = await StreamToStringAsync(stream);
            throw new ApiException
            {
                StatusCode = (int)response.StatusCode,
                Content = content
            };
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
