using Ccf.Ck.SysPlugins.Recorders.Postman.Models;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    public class PostmanMethodBuilder : PostmanBuilder
    {
        public PostmanMethodBuilder(RequestContent postmanRequestContent)
        {
            this.postmanRequestContent = postmanRequestContent;
        }

        public PostmanMethodBuilder AddMethod(string methodName)
        {
            this.postmanRequestContent.Method = methodName;
            return this;
        }
    }
}
