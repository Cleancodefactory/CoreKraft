using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Postman
{
    public class PostmanImp : IRequestRecorder
    {
        public Task<StringBuilder> GetFinalResult()
        {
            throw new NotImplementedException();
        }

        public Task HandleRequest(HttpRequest request, InputModel inputModel)
        {
            throw new NotImplementedException();
        }
    }
}
