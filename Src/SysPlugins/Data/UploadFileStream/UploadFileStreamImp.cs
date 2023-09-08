using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Data.UploadFileStream.BaseClasses;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Data.UploadFileStream
{
    public class UploadFileStreamImp : ISystemPlugin
    {
        public IProcessingContext Execute(
            LoadedNodeSet loaderContext, 
            IProcessingContext processingContext, 
            IPluginServiceManager pluginServiceManager, 
            IPluginsSynchronizeContextScoped contextScoped, 
            INode currentNode)
        {

            IHttpContextAccessor httpContextAccessor = pluginServiceManager.GetService<IHttpContextAccessor>(typeof(HttpContextAccessor));
            HttpRequest request = httpContextAccessor.HttpContext.Request;
            UploadFileBase uploadFile;
            if (request.ContentLength == null && request.Headers["Transfer-Encoding"] == "chunked")
            {
                uploadFile = new UploadFileForm();
            }
            else
            {
                uploadFile = new UploadFileMultipart();
            }
            
            return uploadFile.Execute(request, processingContext).Result;            
        }

        public Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return Task.FromResult< IPluginsSynchronizeContextScoped>(new UploadFileStreamSynchronizeContext());
        }
    }
}
