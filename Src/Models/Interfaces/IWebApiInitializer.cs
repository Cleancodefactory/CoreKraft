using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ccf.Ck.Models.Interfaces
{
    public interface IWebApiInitializer
    {
        void InitializeServices(IServiceCollection services, bool enableSwagger);
        void InitializeBuilder(IApplicationBuilder app, bool enableSwagger);
    }
}
