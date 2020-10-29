using Ccf.Ck.Models.NodeRequest;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IRequestRecorder
    {
        Task HandleRequest(HttpRequest request);
        Task<string> GetFinalResult();
    }
}
