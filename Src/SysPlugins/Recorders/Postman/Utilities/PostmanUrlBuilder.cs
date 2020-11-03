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
                                            List<string> hostSegments, Dictionary<string, string> query)
        {
            this.postmanRequestContent.PostmanUrl = new PostmanUrlSection();
            this.postmanRequestContent.PostmanUrl.Raw = url;
            this.postmanRequestContent.PostmanUrl.Protocol = protocol;
            this.postmanRequestContent.PostmanUrl.PathSegments = segments;
            this.postmanRequestContent.PostmanUrl.HostSegments = hostSegments;
            this.postmanRequestContent.PostmanUrl.Queries = new List<Dictionary<string, string>>();
            this.postmanRequestContent.PostmanUrl.Queries.Add(query);

            return this;
        }
    }
}
