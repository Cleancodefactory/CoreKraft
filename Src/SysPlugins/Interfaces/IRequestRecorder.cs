using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IRequestRecorder
    {
        public bool IsRunning { get; set; }
        Task HandleRequest(HttpRequest request);
        Task<string> GetFinalResult();
    }
}
