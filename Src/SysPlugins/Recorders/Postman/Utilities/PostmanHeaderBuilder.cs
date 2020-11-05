using Ccf.Ck.SysPlugins.Recorders.Postman.Models;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    public class PostmanHeaderBuilder : PostmanBuilder
    {
        public PostmanHeaderBuilder(RequestContent postmanRequestContent)
        {
            this.postmanRequestContent = postmanRequestContent;
        }

        public PostmanHeaderBuilder AddHeader(List<PostmanHeaderSection> postmanHeaderSections)
        {
            this.postmanRequestContent.Headers = postmanHeaderSections;
            return this;
        }
    }
}
