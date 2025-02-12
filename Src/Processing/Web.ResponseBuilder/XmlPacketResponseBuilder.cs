using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class XmlPacketResponseBuilder : HttpResponseBuilder
    {
        public XmlPacketResponseBuilder(IProcessingContextCollection processingContextCollection) : base(processingContextCollection)
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

        protected override async Task WriteToResponseBodyAsync(HttpContext context)
        {
            //Write the collected data into the response
            XDocument doc = new XDocument();

            XElement packet = new XElement("packet");
            doc.Add(packet);
            XElement status;

            IReturnStatus acc_status = ReturnStatus.Combine(_ProcessingContextCollection.ProcessingContexts.Select(pc => pc.ReturnModel.Status));

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, // Handles reference loops
                //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, // Ignores null values
                Converters =
                {
                    new TypeKeyDictionaryConverter<object>() // Custom dictionary key converter
                }
            };

            if (true) //TODO:
            {
                status = new XElement("status");
                packet.Add(status);
                status.Add(new XAttribute("issuccessful", acc_status.IsSuccessful ? 1 : 0));

                // Serialize StatusResults if not null or empty
                if (acc_status.StatusResults != null && acc_status.StatusResults.Count > 0)
                {
                    string jsonStatusResults = JsonSerializer.Serialize(acc_status.StatusResults, options);
                    status.Add(new XElement("messages", new XCData(jsonStatusResults)));
                }

                // Add ReturnUrl if not empty
                if (!string.IsNullOrEmpty(acc_status.ReturnUrl))
                {
                    status.Add(new XElement("returnurl", new XCData(acc_status.ReturnUrl)));
                }

                // Construct the message section
                string errorMessages = acc_status.StatusResults?
                    .Where(sr => sr.StatusResultType == EStatusResult.StatusResultError)
                    .Aggregate("", (a, sr) => a + sr.Message) ?? "";

                status.Add(new XElement("message", new XCData(errorMessages)));

            }

            foreach (IProcessingContext processingContext in _ProcessingContextCollection.ProcessingContexts)
            {
                if (processingContext.ReturnModel.Data != null)
                {
                    string statePropertyName = "state";
                    if (_KraftGlobalConfiguration.GeneralSettings.DataStatePropertyName != null)
                    {
                        statePropertyName = _KraftGlobalConfiguration.GeneralSettings.DataStatePropertyName;
                    }
                    if (_KraftGlobalConfiguration.GeneralSettings.RemovePropertyState)
                    {
                        PropertyRemover.RemoveProperty(processingContext.ReturnModel.Data, statePropertyName);
                    }

                    string jsonData = System.Text.Json.JsonSerializer.Serialize(processingContext.ReturnModel.Data, options);

                    packet.Add(new XElement("data",
                        new XAttribute("sid", GetServerIdentifier(processingContext)),
                        new XCData(jsonData)
                    ));
                }


                if (processingContext.ReturnModel.ExecutionMeta != null)
                {
                    EMetaInfoFlags infoFlag = processingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.MetaLoggingEnumFlag;
                    if (infoFlag.HasFlag(EMetaInfoFlags.Output)) // Flag for output 
                    {
                        string jsonMeta = JsonSerializer.Serialize(processingContext.ReturnModel.ExecutionMeta, options);
                        packet.Add(new XElement("meta", new XCData(jsonMeta)));
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
                    string jsonLookupData = JsonSerializer.Serialize(processingContext.ReturnModel.LookupData, options);

                    packet.Add(new XElement("lookups", new XCData(jsonLookupData)));
                }

                //TODO Robert when the client configuration understands multiple contexts
            }

            await context.Response.WriteAsync(doc.ToString());
        }

        private object GetServerIdentifier(IProcessingContext processingContext)
        {
            return processingContext.InputModel.Module + "/" + processingContext.InputModel.NodeSet + "/" + processingContext.InputModel.Nodepath;
        }
    }

    public class TypeKeyDictionaryConverter<TValue> : JsonConverter<Dictionary<Type, TValue>>
    {
        public override Dictionary<Type, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserialization is not supported for Type dictionary keys.");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Type, TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.AssemblyQualifiedName ?? kvp.Key.FullName ?? "UnknownType");
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }

    public class PropertyRemover
    {
        public static void RemoveProperty(object obj, string propertyToRemove)
        {
            if (obj is Dictionary<string, object> dict)
            {
                // Remove the target property from the dictionary
                dict.Remove(propertyToRemove);

                // Recursively process nested dictionaries and lists
                foreach (var key in new List<string>(dict.Keys)) // Create a copy of keys to avoid modification issues
                {
                    RemoveProperty(dict[key], propertyToRemove);
                }
            }
            else if (obj is List<object> list)
            {
                // Recursively process each item in the list
                for (int i = 0; i < list.Count; i++)
                {
                    RemoveProperty(list[i], propertyToRemove);
                }
            }
        }
    }

}
