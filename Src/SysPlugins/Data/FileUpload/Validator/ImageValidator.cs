using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileUpload.Validator
{
    internal class ImageValidator : IDisposable
    {
        private bool _IsDisposed = false;

        private Dictionary<string, byte[]> _ValidatorsByName = new Dictionary<string, byte[]>()
        {
            { "jpeg", new byte[] { 0xFF, 0xD8 } },
            { "png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            { "bmp", new byte[] { 0x42, 0x4D } },
            { "gif", new byte[] { 0x47, 0x49, 0x46 } },
        };

        public void Dispose()
        {
            if (!_IsDisposed)
            {
                _ValidatorsByName = null;
                GC.SuppressFinalize(this);

                _IsDisposed = !_IsDisposed;
            }
        }

        internal bool ValidateIfStreamIsImage(Stream stream, string type)
        {
            if (stream == null || string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            byte[] imageBytes = null;

            _ValidatorsByName.TryGetValue(type.ToLower(), out imageBytes);

            if (imageBytes == null)
            {
                return false;
            }

            byte[] bytesToVerify = new byte[imageBytes.Length];

            stream.Read(bytesToVerify, 0, bytesToVerify.Length);

            for (int i = 0; i < imageBytes.Length; i++)
            {
                if (imageBytes[i] != bytesToVerify[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal IEnumerable<string> GetValidMimeTypes
        {
            get { return _ValidatorsByName.Keys; }
        }
    }
}