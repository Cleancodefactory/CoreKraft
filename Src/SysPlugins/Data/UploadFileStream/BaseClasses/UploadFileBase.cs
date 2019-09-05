using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Data.UploadFileStream.BaseClasses
{
    abstract class UploadFileBase
    {
        abstract internal Task<Interfaces.ContextualBasket.IProcessingContext> Execute(HttpRequest request, Interfaces.ContextualBasket.IProcessingContext processingContext);

        protected static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }
}
