using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Ccf.Ck.Web.Middleware.Aws
{
    public class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _Region;
        private readonly string _SecretName;

        public AmazonSecretsManagerConfigurationProvider(string region, string secretName)
        {
            _Region = region;
            _SecretName = secretName;
        }

        public override void Load()
        {
            var secret = GetSecret();

            Data = JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
        }

        private string GetSecret()
        {
            var request = new GetSecretValueRequest
            {
                SecretId = _SecretName,
                VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
            };

            using (var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_Region)))
            {
                var response = client.GetSecretValueAsync(request).Result;

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
    }
}
