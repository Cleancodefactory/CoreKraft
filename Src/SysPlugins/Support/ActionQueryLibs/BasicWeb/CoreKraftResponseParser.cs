using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    internal class CoreKraftResponseParser
    {
        public static T? Convert<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            string? parentElement = null;
            bool isSuccessful = true;
            string? errorMessage = null;
            List<ErrorMessage>? errorMessages = new List<ErrorMessage>();

            using (XmlReader reader = XmlReader.Create(new StringReader(content)))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (string.Equals(reader.Name, "message", StringComparison.OrdinalIgnoreCase))
                        {
                            parentElement = "message";
                        }
                        else if (string.Equals(reader.Name, "messages", StringComparison.OrdinalIgnoreCase))
                        {
                            parentElement = "messages";
                        }
                        else if (string.Equals(reader.Name, "data", StringComparison.OrdinalIgnoreCase))
                        {
                            parentElement = "data";
                        }
                        else if (string.Equals(reader.Name, "status", StringComparison.OrdinalIgnoreCase))
                        {
                            parentElement = "status";

                            string? isSuccessfulAsString = reader.GetAttribute("issuccessful");

                            isSuccessful = isSuccessfulAsString == "1";
                        }
                    }

                    if (reader.NodeType == XmlNodeType.CDATA)
                    {
                        if (string.Equals(parentElement, "data", StringComparison.OrdinalIgnoreCase) && isSuccessful == true)
                        {
                            string cDataContent = reader.ReadContentAsString();
                            if (!string.IsNullOrEmpty(cDataContent))
                            {
                                if (typeof(T) == typeof(string))
                                {
                                    return (T)(object)cDataContent;
                                }
                                return JsonSerializer.Deserialize<T>(cDataContent);
                            }
                        }
                        else if (string.Equals(parentElement, "binarydata", StringComparison.OrdinalIgnoreCase) && isSuccessful == true)
                        {
                            string cDataContent = reader.ReadContentAsString();
                            if (!string.IsNullOrEmpty(cDataContent))
                            {
                                if (typeof(T) == typeof(string))
                                {
                                    return (T)(object)cDataContent;
                                }
                                return JsonSerializer.Deserialize<T>(cDataContent);
                            }
                        }
                        else if (string.Equals(parentElement, "message", StringComparison.OrdinalIgnoreCase) && isSuccessful == false)
                        {
                            errorMessage = reader.ReadContentAsString();
                        }
                        else if (string.Equals(parentElement, "messages", StringComparison.OrdinalIgnoreCase) && isSuccessful == false)
                        {
                            errorMessages = JsonSerializer.Deserialize<List<ErrorMessage>>(reader.ReadContentAsString());
                        }
                    }
                }
            }

            if (isSuccessful == false)
            {
                throw new CoreKraftResponseException(errorMessage, errorMessages);
            }

            return default;
        }
    }
}
