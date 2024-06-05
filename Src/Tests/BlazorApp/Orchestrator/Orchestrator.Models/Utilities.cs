using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace Orchestrator.Models
{
    public static class Utilities
    {
        public static T ParseContentToT<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }
            
            string parentElement = null;
            bool isSuccessful = true;
            string errorMessage = null;
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();

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

                            string isSuccessfulAsString = reader.GetAttribute("issuccessful");

                            isSuccessful = isSuccessfulAsString == "1";
                        }
                    }

                    if (reader.NodeType == XmlNodeType.CDATA)
                    {
                        if (string.Equals(parentElement, "data", StringComparison.OrdinalIgnoreCase) && isSuccessful == true)
                        {
                            string cDataContent = reader.ReadContentAsString();

                            return JsonSerializer.Deserialize<T>(cDataContent);
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
