using Ccf.Ck.NodePlugins.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using OpenAI_API;
using OpenAI_API.Chat;
using System;

namespace Ccf.Ck.NodePlugins.OrchestratorHelper
{
    public class OrchestratorHelperMainImp : NodePluginBase<OrchestratorHelperContext>
    {
        protected override void ExecuteRead(INodePluginReadContext pr) => throw new NotImplementedException();

        protected override void ExecuteWrite(INodePluginWriteContext pw)
        {
            string input = pw.Evaluate("input").Value?.ToString() ?? null;
            string template = pw.Evaluate("template").Value?.ToString() ?? null;
            string prompt = pw.Evaluate("prompt").Value?.ToString() ?? null;
            string systemMessage = pw.Evaluate("system_message").Value?.ToString() ?? null;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"Parameter {nameof(input)} is required.");
            }

            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException($"Parameter {nameof(template)} is required.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException($"Parameter {nameof(prompt)} is required.");
            }

            OpenAIAPI openAIAPI = new OpenAIAPI("API-KEY");

            Conversation chat = openAIAPI.Chat.CreateConversation(new ChatRequest() { ResponseFormat = "json_object" });
            if (!string.IsNullOrWhiteSpace(systemMessage))
            {
                chat.AppendSystemMessage(systemMessage);
            }
            chat.AppendUserInput($"This is the patinet json input:{Environment.NewLine}{input}{Environment.NewLine}This is the FHIR template:{Environment.NewLine}{template}{Environment.NewLine}Use this hints if any:{Environment.NewLine}{prompt}");
            string response = chat.GetResponseFromChatbotAsync().Result;

            pw.Row.Add("message", response);
        }

        private IIndirectCallService CreateIndirectCallService(INodePluginContext context)
        {
            IIndirectCallService indirectService = context.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));

            if (indirectService == null)
            {
                throw new Exception($"Missing required service {nameof(indirectService)}.");
            }

            return indirectService;
        }
    }
}