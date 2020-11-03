using Ccf.Ck.SysPlugins.Recorders.Postman.Models;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    public class PostmanBuilder
    {
        protected RequestContent postmanRequestContent = new RequestContent();

        public PostmanMethodBuilder MethodBuilder => new PostmanMethodBuilder(postmanRequestContent);
        public PostmanBodyBuilder BodyBuilder => new PostmanBodyBuilder(postmanRequestContent);
        public PostmanHeaderBuilder HeaderBuilder => new PostmanHeaderBuilder(postmanRequestContent);
        public PostmanUrlBuilder UrlBuilder => new PostmanUrlBuilder(postmanRequestContent);

        public static implicit operator RequestContent(PostmanBuilder pb)
        {
            return pb.postmanRequestContent;
        }
    }
}
