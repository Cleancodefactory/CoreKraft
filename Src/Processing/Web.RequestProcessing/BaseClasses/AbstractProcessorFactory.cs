using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;

namespace Ccf.Ck.Processing.Web.Request.BaseClasses
{
    internal abstract class AbstractProcessorFactory
    {
        internal abstract ProcessorBase CreateProcessor(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, INodeSetService nodesSetService);
    }
}
