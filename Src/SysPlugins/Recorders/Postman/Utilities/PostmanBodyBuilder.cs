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
            body.Mode = mode;
            body.Raw = raw;

            this.postmanRequestContent.PostmanBody = body;

            return this;
        }
    }
}
