using Microsoft.AspNetCore.Components;
using Orchestrator.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orchestrator.Client.Services
{
    public class MapService
    {
        private readonly HttpClient _HttpClient;

        public MapService(HttpClient httpClient, NavigationManager navigationManager)
        {
            _HttpClient = httpClient;
            _HttpClient.BaseAddress = new Uri(navigationManager.BaseUri);
        }

        public async Task<FhirResponse> StartMap(string input, string template, string prompt, string systemMessage)
        {
            return await SendRequestAsync<FhirResponse>("/node/write/Orchestrator/Fhir/Create", HttpMethod.Post, new
            {
                input,
                template,
                prompt,
                system_message = systemMessage,
                state = "1"
            });
        }

        private async Task<T> SendRequestAsync<T>(string url, HttpMethod method, object? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException($"Parameter ${nameof(url)} is required.");
            }
            
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, url))
            {
                if (method ==  HttpMethod.Post)
                {
                    httpRequestMessage.Content = JsonContent.Create(parameters,
                        new MediaTypeHeaderValue("application/json"),
                        new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                }
                
                HttpResponseMessage httpResponseMessage = await _HttpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Error sending request the status code was: {httpResponseMessage.StatusCode}. The response message was: {await httpResponseMessage.Content.ReadAsStringAsync()}");
                }

                string content = await httpResponseMessage.Content.ReadAsStringAsync();

                return Utilities.ParseContentToT<T>(content);
            }
        }
    }
}
