using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Xml.Linq;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class XmlPacketResponseBuilder : HttpResponseBuilder
    {
        public XmlPacketResponseBuilder(IProcessingContextCollection processingContextCollection): base(processingContextCollection)
        {
        }

        protected override void WriteToResponseHeaders(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                HttpResponse response = context.Response;

                // Disable caching for all Kraft responses
                response.Headers["Cache-Control"] = "no-cache, no-store";
                response.Headers["Pragma"] = "no-cache";
                response.Headers["Expires"] = "-1";

                //set xml content type        
                response.ContentType = "text/xml";
            }   
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
                if (!string.IsNullOrEmpty(acc_status.ReturnUrl))
                {
                    status.Add(new XElement("returnurl", new XCData(acc_status.ReturnUrl)));
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

                    packet.Add(new XElement("data", new XAttribute("sid", GetServerIdentifier(processingContext)), new XCData(JsonConvert.SerializeObject(processingContext.ReturnModel.Data, settings))));
                }

                
                if (processingContext.ReturnModel.ExecutionMeta != null)
                {
                    EMetaInfoFlags infoFlag = processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.MetaLoggingEnumFlag;
                    if (infoFlag.HasFlag(EMetaInfoFlags.Output)) // Flag for output 
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        settings.Error = (serializer, err) =>
                        {
                            err.ErrorContext.Handled = true;
                        };

                        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                        packet.Add(new XElement("meta", new XCData(JsonConvert.SerializeObject(processingContext.ReturnModel.ExecutionMeta, settings))));
                    }
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
                //TODO Robert when the client configuration understands multiple contexts
            }

            context.Response.WriteAsync(doc.ToString()).Wait();
        }

        private object GetServerIdentifier(IProcessingContext processingContext)
        {
            return processingContext.InputModel.Module + "/" + processingContext.InputModel.NodeSet + "/" + processingContext.InputModel.Nodepath;
        }
    }
}
