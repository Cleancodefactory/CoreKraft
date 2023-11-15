using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Ccf.Ck.Web.Middleware
{
    internal static class KraftStaticFiles
    {
        internal static void RegisterStaticFiles(IApplicationBuilder builder, string modulePath, string startNode, string resourceSegmentName, string type, ICorsService corsService, ICorsPolicyProvider corsPolicyProvider)
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
                        if (corsPolicyProvider != null)
                        {
                            CorsPolicy policy = corsPolicyProvider.GetPolicyAsync(ctx.Context, "CorsPolicy")
                                                .ConfigureAwait(false)
                                                .GetAwaiter().GetResult();
                            CorsResult corsResult = corsService.EvaluatePolicy(ctx.Context, policy);
                            corsService.ApplyResult(corsResult, ctx.Context.Response);
                        }
                    }
                });
            }
        }
    }
}
