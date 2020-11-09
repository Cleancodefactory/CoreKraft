using Ccf.Ck.SysPlugins.Recorders.Postman.Models;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    public class PostmanUrlBuilder : PostmanBuilder
    {
        public PostmanUrlBuilder(RequestContent postmanRequestContent)
        {
            this.postmanRequestContent = postmanRequestContent;
        }

        public PostmanUrlBuilder AddUrlData(string url, string protocol, List<string> segments,
                                            List<string> hostSegments, List<PostmanQuerySection> query)
        {
            this.postmanRequestContent.PostmanUrl = new PostmanUrlSection();
            this.postmanRequestContent.PostmanUrl.Raw = url;
            this.postmanRequestContent.PostmanUrl.Protocol = protocol;
            this.postmanRequestContent.PostmanUrl.HostSegments = hostSegments;
            this.postmanRequestContent.PostmanUrl.PathSegments = segments;
            this.postmanRequestContent.PostmanUrl.Queries  = query;

            return this;
        }
    }
}
