using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Ccf.Ck.Web.Middleware
{
    internal static class KraftStaticFiles
    {
        internal static void RegisterStaticFiles(IApplicationBuilder builder, string modulePath, string startNode, string resourceSegmentName, string type)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(modulePath);
            foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
            {
                if (Directory.Exists(Path.Combine(dirInfo.FullName, type)))
                {
                    builder.UseStaticFiles(new StaticFileOptions
                    {
                        ServeUnknownFileTypes = false,
                        DefaultContentType = "image/png",
                        FileProvider = new PhysicalFileProvider(Path.Combine(dirInfo.FullName, type)),
                        RequestPath = new PathString($"/{startNode}/{resourceSegmentName}/{dirInfo.Name}/{type}"),
                        OnPrepareResponse = ctx =>
                        {
                            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                        }
                    });
                }
            }
        }
    }
}
