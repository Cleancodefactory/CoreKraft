using Ccf.Ck.SysPlugins.Recorders.Postman.Models;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    public class PostmanBodyBuilder : PostmanBuilder
    {
        public PostmanBodyBuilder(RequestContent postmanRequestContent)
        {
            this.postmanRequestContent = postmanRequestContent;
        }

        public PostmanBodyBuilder AddBody(string mode, string raw)
        {
            PostmanBodySection body = new PostmanBodySection();

            if (mode == null && raw == null)
            {
                body.Raw = string.Empty;
                body.Mode = string.Empty;
            }

            body.Raw = raw;
            body.Mode = mode;

            this.postmanRequestContent.PostmanBody = body;

            return this;
        }
    }
}
