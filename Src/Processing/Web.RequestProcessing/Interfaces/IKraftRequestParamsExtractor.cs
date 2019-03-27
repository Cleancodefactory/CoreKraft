using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Web.Settings;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Web.Request.Interfaces
{
    public interface IKraftRequestParamsExtractor
    {
        Task<ProcessingContextCollection> ExtractParamsAsync(HttpContext context, string loaderTypeKey, KraftEnvironmentSettings kraftEnvironmentSettings);
    }
}
