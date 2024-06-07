namespace WebApiCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApiInitializer webApiInitializer = new WebApiInitializer();
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddControllers();

            //// Add services to the container.
            webApiInitializer.InitializeServices(builder.Services, true);

            WebApplication app = builder.Build();
            webApiInitializer.InitializeBuilder(app, true);
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
