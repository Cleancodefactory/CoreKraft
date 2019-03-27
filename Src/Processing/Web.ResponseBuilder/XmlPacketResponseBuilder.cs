using Ccf.Ck.Models.Packet;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Xml.Linq;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using Ccf.Ck.SysPlugins.Interfaces.Packet;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class XmlPacketResponseBuilder : HttpResponseBuilder
    {
        private IProcessingContextCollection _ProcessingContextCollection;

        public XmlPacketResponseBuilder(IProcessingContextCollection processingContextCollection)
        {
            _ProcessingContextCollection = processingContextCollection;
        }

        protected override void WriteToResponseHeaders(HttpContext context)
        {
            HttpResponse response = context.Response;

            // Disable caching for all Kraft responses
            response.Headers["Cache-Control"] = "no-cache, no-store";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "-1";

            //set xml content type        
            response.ContentType = "text/xml";
        }

        protected override void WriteToResponseBody(HttpContext context)
        {
            //Write the collected data into the response
            XDocument doc = new XDocument();

            XElement packet = new XElement("packet");
            doc.Add(packet);
            XElement status;

            IReturnStatus acc_status = ReturnStatus.Combine(_ProcessingContextCollection.ProcessingContexts.Select(pc => pc.ReturnModel.Status));

            if (true) //TODO:
            {
                status = new XElement("status");
                packet.Add(status);
                status.Add(new XAttribute("issuccessful", acc_status.IsSuccessful?1:0));

                // TODO: We construct the status section from the a combination of all statuses of all RetirnModels (which are just 1 for now)
                if (acc_status.StatusResults != null && acc_status.StatusResults.Count > 0)
                {
                    status.Add(new XElement("messages", new XCData(JsonConvert.SerializeObject(acc_status.StatusResults))));
                }

                status.Add(new XElement("message", new XCData(acc_status.StatusResults.Aggregate("",(a,sr) => (sr.StatusResultType == EStatusResult.StatusResultError)?a + sr.Message: a ))));
            }

            foreach (IProcessingContext processingContext in _ProcessingContextCollection.ProcessingContexts)
            {
                if (processingContext.ReturnModel.Data != null)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Error = (serializer, err) =>
                    {
                        err.ErrorContext.Handled = true;
                    };

                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                    packet.Add(new XElement("data", new XCData(JsonConvert.SerializeObject(processingContext.ReturnModel.Data, settings))));
                }

                if (processingContext.ReturnModel.Views != null)
                {
                    XElement views = new XElement("views");
                    foreach (var item in processingContext.ReturnModel.Views)
                    {
                        var v = new XElement(item.Key, new XCData(item.Value.Content));
                        v.Add(new XAttribute("sid", item.Value.SId));
                        views.Add(v);
                    }
                    //views.Add(new XElement("normal", new XCData(processingContext.View)));
                    packet.Add(views);
                }

                if (processingContext.ReturnModel.LookupData != null)
                {
                    packet.Add(new XElement("lookups", new XCData(JsonConvert.SerializeObject(processingContext.ReturnModel.LookupData))));
                }
            }

            context.Response.WriteAsync(doc.ToString()).Wait();
        }
    }
}
