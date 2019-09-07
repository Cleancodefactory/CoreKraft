using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.Models.NodeRequest
{
    public class PostedFile : IPostedFile
    {
        private Func<object, Stream> _OpenFileStream;
        private object _FileSource;
        public PostedFile(string contentType, long length, string name, string fileName, Func<object, Stream> openFileStream, object fileSource)
        {
            ContentType = contentType;
            Length = length;
            Name = name;
            FileName = fileName;
            _OpenFileStream = openFileStream;
            _FileSource = fileSource;
            MetaInfo = new Dictionary<string, object>();
        }
        //
        // Summary:
        //     Gets the raw Content-Type header of the uploaded file.
        public string ContentType { get; private set; }
        //
        // Summary:
        //     Gets the file length in bytes.
        public long Length { get; private set; }
        //
        // Summary:
        //     Gets the name from the Content-Disposition header.
        public string Name { get; private set; }
        //
        // Summary:
        //     Gets the file name from the Content-Disposition header.
        public string FileName { get; private set; }

        /// <summary>
        /// Contains meta info for every downloaded file specific to BindKraft and its nodesets like:
        /// node.childnode, node.childnode.childchildnode and state for the proper action in the module
        /// </summary>
        public Dictionary<string, object> MetaInfo { get; set; }

        //
        // Summary:
        //     Opens the request stream for reading the uploaded file.
        public Stream OpenReadStream()
        {
            return _OpenFileStream(_FileSource);
        }
    }
}
