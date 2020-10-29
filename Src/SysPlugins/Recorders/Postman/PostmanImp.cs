using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Postman
{
    public class PostmanImp : IRequestRecorder
    {
        string _GenResult;

        public Task<string> GetFinalResult()
        {
            return Task.FromResult(_GenResult);
        }

        public Task HandleRequest(HttpRequest request)
        {
            _GenResult = _GenResult + $"{request.Scheme} {request.Path.Value} {Environment.NewLine}";
            return Task.FromResult(_GenResult);
        }
    }
}
