using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;

namespace Ccf.Ck.Processing.Web.Request
{
    public class WebEntrance
    {
        private IServiceProvider _ApplicationServices;
        private KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public WebEntrance(IServiceProvider applicationServices, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _ApplicationServices = applicationServices;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        //public WebEntrance Instance(IServiceProvider applicationServices, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        //{
        //    return new WebEntrance(applicationServices, kraftGlobalConfigurationSettings);
        //}
    }
}