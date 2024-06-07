using Ccf.Ck.Models.Interfaces;

namespace WebApiCore
{
    public class WebApiInitializer : IWebApiInitializer
    {
        public void InitializeServices(IServiceCollection services, bool enableSwagger)
        {
            if (enableSwagger)
            {
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
            }
        }

        public void InitializeBuilder(IApplicationBuilder app, bool enableSwagger)
        {
            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        }
    }
}