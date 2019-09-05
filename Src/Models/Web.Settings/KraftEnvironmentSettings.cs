using System;

namespace Ccf.Ck.Models.Web.Settings
{
    public class KraftEnvironmentSettings
    {
        public KraftEnvironmentSettings(string applicationName, string contentRootPath, string environmentName, string webRootPath)
        {
            ApplicationName = applicationName;
            ContentRootPath = contentRootPath;
            EnvironmentName = environmentName;
            WebRootPath = webRootPath;
        }
        public string ApplicationName { get; }
        public string ContentRootPath { get; }
        public string EnvironmentName { get; }
        public string WebRootPath { get; }

        public bool IsDevelopment()
        {
            if (!string.IsNullOrEmpty(EnvironmentName))
            {
                return EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}