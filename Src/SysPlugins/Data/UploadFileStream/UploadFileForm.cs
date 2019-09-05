using System;
using System.Threading.Tasks;
using Ccf.Ck.SysPlugins.Data.UploadFileStream.BaseClasses;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.Http;

namespace Ccf.Ck.SysPlugins.Data.UploadFileStream
{
    class UploadFileForm : UploadFileBase
    {
        internal override Task<IProcessingContext> Execute(HttpRequest request, IProcessingContext processingContext)
        {
            throw new NotImplementedException();
        }
    }
}
