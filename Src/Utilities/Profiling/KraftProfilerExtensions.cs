using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System;
using System.Data.Common;
using Microsoft.Extensions.Hosting;

namespace Ccf.Ck.Utilities.Profiling
{
    public static class KraftProfilerExtensions
    {
        public static void UseBindKraftProfiler(this IApplicationBuilder builder, IWebHostingEnvironment env, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            builder.UseMiniProfiler();
            //MiniProfiler.Settings.PopupRenderPosition = RenderPosition.Right;
        }

        public static void UseBindKraftProfiler(this IServiceCollection services)
        {
            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                options.PopupMaxTracesToShow = 15;
                options.PopupRenderPosition = RenderPosition.BottomRight;
                options.MaxUnviewedProfiles = 20;
                options.StackMaxLength = 120;

                //// (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                //// (default is everyone can access profilers)
                //options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;

                //// (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
                //// (default is everything should be profiled)
                //options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                //// (Optional) Profiles are stored under a user ID, function to get it:
                //// (default is null, since above methods don't use it by default)
                //options.UserIdProvider = request => MyGetUserIdFunction(request);

                //// (Optional) Swap out the entire profiler provider, if you want
                //// (default handles async and works fine for almost all appliations)
                //options.ProfilerProvider = new MyProfilerProvider();
            });
        }

        /// <summary>
        /// Returns an <see cref="Timing"/> (<see cref="IDisposable"/>) that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profiler">The current profiling session or null.</param>
        /// <param name="name">A descriptive name for the code that is encapsulated by the resulting Timing's lifetime.</param>
        /// <returns>the profile step</returns>
        public static KraftTiming Step(this KraftProfiler profiler, string name)
        {
            return new KraftTiming(MiniProfiler.Current.Step(name));
        }

        /// <summary>
        /// Returns an <see cref="Timing"/> (<see cref="IDisposable"/>) that will time the code between its creation and disposal.
        /// Will only save the <see cref="Timing"/> if total time taken exceeds <paramref name="minSaveMs" />.
        /// </summary>
        /// <param name="profiler">The current profiling session or <c>null</c>.</param>
        /// <param name="name">A descriptive name for the code that is encapsulated by the resulting Timing's lifetime.</param>
        /// <param name="minSaveMs">The minimum amount of time that needs to elapse in order for this result to be recorded.</param>
        /// <param name="includeChildren">Should the amount of time spent in child timings be included when comparing total time
        /// profiled with <paramref name="minSaveMs"/>? If true, will include children. If false will ignore children.</param>
        /// <returns></returns>
        /// <remarks>If <paramref name="includeChildren"/> is set to true and a child is removed due to its use of StepIf, then the 
        /// time spent in that time will also not count for the current StepIf calculation.</remarks>
        public static KraftTiming StepIf(this KraftProfiler profiler, string name, decimal minSaveMs, bool includeChildren = false)
        {
            return new KraftTiming(MiniProfiler.Current.StepIf(name, minSaveMs, includeChildren));
        }

        public static DbConnection ProfiledDbConnection(this KraftProfiler profiler, DbConnection conn)
        {
            return new StackExchange.Profiling.Data.ProfiledDbConnection(conn, MiniProfiler.Current);
        }

    }
}
