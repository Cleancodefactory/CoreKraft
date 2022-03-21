using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace Ccf.Ck.Models.NodeRequest
{
    public class PostedFile : IPostedFile
    {
        private Func<object, Stream> _OpenFileStream;
        private object _FileSource;
        public PostedFile(IPostedFile ipf, string contentType = null, string name = null, string fileName = null) {
            ContentType = contentType ?? ipf.ContentType;
            Length = ipf.Length;
            Name = name ?? ipf.Name;
            FileName = fileName ?? ipf.FileName;
            _FileSource = ipf;
            // TODO using
            _OpenFileStream = pf => ((IPostedFile)pf).OpenReadStream();
            MetaInfo = new Dictionary<string, object>();

            // Content type autodetect
            if (ContentType == "application/octet-stream" || contentType == null) {
                if (new FileExtensionContentTypeProvider().TryGetContentType(FileName, out string _contentType)) {
                    ContentType = _contentType;
                };
            }
        }
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
        /// NOT SERIOUSLY USED can be REPURPOSED. Was and is still used by the ProcessorMultipart 
        /// (superseded by ProcessorMultipartEx)
        /// </summary>
        public Dictionary<string, object> MetaInfo { get; set; }

        //
        // Summary:
        //     Opens the request stream for reading the uploaded file.
        public Stream OpenReadStream()
        {
            return _OpenFileStream(_FileSource);
        }

        #region Helpers
        public byte[] ToBytes() {
            using var strm = OpenReadStream();
            using var mstrm = new MemoryStream();
            strm.CopyTo(mstrm);
            return mstrm.ToArray();
        }
        #endregion

        #region Static constructors
        public static PostedFile FromFile(string filePath, string contentType) {
            return FromFile(filePath, null, contentType);
        }
        public static PostedFile FromFile(string filePath, string name = null, string contentType = null) {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath is null or empty");
            if (!Path.IsPathFullyQualified(filePath)) {
                throw new ArgumentException($"The file path {filePath} is not fully qualified.");
            }
            var _name = name;
            if (_name == null) {
                _name = Path.GetFileNameWithoutExtension(filePath);
            }
            var fileName = Path.GetFileName(filePath);
            var _contentType = contentType;
            if (_contentType == null) {
                var mfe = new FileExtensionContentTypeProvider();
                if (mfe.TryGetContentType(filePath, out string ct)) {
                    _contentType = ct;
                } else {
                    _contentType = "application/octet-stream";
                }
            }
            var _length = 0L;
            if (File.Exists(filePath)) {
                var fi = new FileInfo(filePath);
                _length = fi.Length;
            }
            return new PostedFile(_contentType,_length, _name, fileName, fs => File.OpenRead(fs as string), filePath);
        }
        public static PostedFile FromBytes(string contentType, string name, string fileName, byte[] bytes) {
            return new PostedFile(contentType, bytes.Length, name, fileName, bts => new MemoryStream(bts as byte[]), bytes);
        }

        #endregion

        #region conversions
        public static implicit operator byte[](PostedFile pf) {
            return pf.ToBytes();
        }
        public static explicit operator PostedFile(byte[] bytes) {
            return PostedFile.FromBytes("application/octet-stream", "unnamed", "unnamed", bytes);
        }
        #endregion
    }
}
