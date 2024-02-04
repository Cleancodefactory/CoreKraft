using Microsoft.Extensions.Configuration;

namespace Ccf.Ck.Web.Middleware.Aws
{
    public class AmazonSecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly string _Region;
        private readonly string _SecretName;

        public AmazonSecretsManagerConfigurationSource(string region, string secretName)
        {
            _Region = region;
            _SecretName = secretName;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AmazonSecretsManagerConfigurationProvider(_Region, _SecretName);
        }
    }
}
