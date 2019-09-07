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
        

        public async Task<IProcessingContext> ExecuteAsync(
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
            
            return await uploadFile.Execute(request, processingContext);            
        }

        public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return await Task.FromResult(new UploadFileStreamSynchronizeContext());
        }
    }
}
