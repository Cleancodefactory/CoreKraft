using Microsoft.AspNetCore.Builder;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IUseMiddleWare
    {
        void Use(IApplicationBuilder app);
    }
}
