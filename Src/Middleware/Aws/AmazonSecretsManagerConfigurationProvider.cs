using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Ccf.Ck.Web.Middleware.Aws
{
    public class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _Region;
        private readonly string _SecretName;
        private readonly Dictionary<string, string> _Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _Paths = new Stack<string>();

        public AmazonSecretsManagerConfigurationProvider(string region, string secretName)
        {
            _Region = region;
            _SecretName = secretName;
        }

        public override void Load()
        {
            string secret = GetSecret();
             
            Data = ParseStream(secret);
        }

        private string GetSecret()
        {
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = _SecretName,
                VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
            };

            using (var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_Region)))
            {
                GetSecretValueResponse response = client.GetSecretValueAsync(request).Result;

                string secretString;
                if (response.SecretString != null)
                {
                    secretString = response.SecretString;
                }
                else
                {
                    var memoryStream = response.SecretBinary;
                    var reader = new StreamReader(memoryStream);
                    secretString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                }
                return secretString;
            }
        }

        private Dictionary<string, string> ParseStream(string input)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            //using (var reader = new StreamReader(input))
            using (JsonDocument doc = JsonDocument.Parse(input, jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }
                VisitObjectElement(doc.RootElement);
            }

            return _Data;
        }

        private void VisitObjectElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            SetNullIfElementIsEmpty(isEmpty);
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_Paths.Count > 0);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitObjectElement(value);
                    break;

                case JsonValueKind.Array:
                    VisitArrayElement(value);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = _Paths.Peek();
                    if (_Data.ContainsKey(key))
                    {
                        throw new FormatException($"A duplicate key '{key}' was found.");
                    }
                    _Data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
            }
        }

        private void VisitArrayElement(JsonElement element)
        {
            int index = 0;

            foreach (JsonElement arrayElement in element.EnumerateArray())
            {
                EnterContext(index.ToString());
                VisitValue(arrayElement);
                ExitContext();
                index++;
            }

            SetNullIfElementIsEmpty(isEmpty: index == 0);
        }

        private void SetNullIfElementIsEmpty(bool isEmpty)
        {
            if (isEmpty && _Paths.Count > 0)
            {
                _Data[_Paths.Peek()] = null;
            }
        }

        private void EnterContext(string context) =>
            _Paths.Push(_Paths.Count > 0 ?
                _Paths.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _Paths.Pop();
    }
}
