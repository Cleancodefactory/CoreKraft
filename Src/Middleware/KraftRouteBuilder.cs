using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Web.Middleware
{
    /// <summary>
    /// Maps the Kraft routings where only the entry point is configurable
    /// </summary>
    internal class KraftRouteBuilder
    {
        internal static IRouter MakeRouter(IApplicationBuilder builder, RouteHandler kraftRoutesHandler, string kraftUrlSegment)
        {
            // create route builder with route handler
            RouteBuilder kraftRoutesBuilder = new RouteBuilder(builder, kraftRoutesHandler);

            //we expect the routing to be like this:
            //domain.com/startnode/<read|write>/module/nodeset/<nodepath>?lang=de
            kraftRoutesBuilder.MapRoute(
                name: "corekraft_warmup_route",
                template: kraftUrlSegment + "/" + Constants.RouteSegmentConstants.RouteWarmup,
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenWarmup }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_batch_routes",
                template: kraftUrlSegment + "/" + Constants.RouteSegmentConstants.RouteBatch,
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenBatch }
            );
            
            kraftRoutesBuilder.MapRoute(
                name: "corekraft_signal_read_route",
                template: kraftUrlSegment + "/read/" + Constants.RouteSegmentConstants.RouteModuleSignal + "/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteModuleSignalParameter + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenSignalRead }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_signal_write_route",
                template: kraftUrlSegment + "/write/" + Constants.RouteSegmentConstants.RouteModuleSignal + "/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteModuleSignalParameter + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenSignalWrite }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_signal_route",
                template: kraftUrlSegment + "/" + Constants.RouteSegmentConstants.RouteModuleSignal + "/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteModuleSignalParameter + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenSignal }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_view_route",
                template: kraftUrlSegment + "/view/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteNodeset + "}/{" + Constants.RouteSegmentConstants.RouteNodepath + "}/{" + Constants.RouteSegmentConstants.RouteBindingkey + "}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenView }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_single_read_route",
                template: kraftUrlSegment + "/read/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteNodeset + "}/{"+ Constants.RouteSegmentConstants.RouteNodepath +"?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenRead }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_single_new_route",
                template: kraftUrlSegment + "/new/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteNodeset + "}/{" + Constants.RouteSegmentConstants.RouteNodepath + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenNew }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_single_write_route",
                template: kraftUrlSegment + "/write/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteNodeset + "}/{" + Constants.RouteSegmentConstants.RouteNodepath + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenWrite }
            );

            kraftRoutesBuilder.MapRoute(
                name: "corekraft_single_route",
                template: kraftUrlSegment + "/{" + Constants.RouteSegmentConstants.RouteModule + "}/{" + Constants.RouteSegmentConstants.RouteNodeset + "}/{" + Constants.RouteSegmentConstants.RouteNodepath + "?}",
                defaults: null,
                constraints: null,
                dataTokens: new { key = Constants.RouteSegmentConstants.RouteDataTokenSingle }
            );

            IRouter kraftRouter = kraftRoutesBuilder.Build();
            return kraftRouter;
        }

        public class FromValuesListConstraint : IRouteConstraint
        {
            private readonly List<string> _Values;

            public FromValuesListConstraint(params string[] values)
            {
                _Values = values.Select(x => x.ToLower()).ToList();
            }

            public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
            {
                string value = values[routeKey].ToString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return _Values.Contains(string.Empty);
                }

                return _Values.Contains(value.ToLower());
            }
        }
    }
}
