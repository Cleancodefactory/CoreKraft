using Microsoft.AspNetCore.Http;

namespace Ccf.Ck.SysPlugins.Interfaces.Packet
{
    public interface IHttpResponseBuilder
    {       
        void GenerateResponse(HttpContext context);
    }
}
