using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.Models.NodeRequest
{
    public interface IPostedFile
    {
        string ContentType { get; }
        string FileName { get; }
        long Length { get; }
        string Name { get; }
        Dictionary<string, object> MetaInfo { get; set; }
        Stream OpenReadStream();
    }
}