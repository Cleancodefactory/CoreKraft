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
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(modulePath, type));
            if (dirInfo.Exists)
            {
                builder.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "image/png",
                    FileProvider = new PhysicalFileProvider(dirInfo.FullName),
                    RequestPath = new PathString($"/{startNode}/{resourceSegmentName}/{directoryInfo.Name}/{type}"),
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    }
                });
            }
        }
    }
}
