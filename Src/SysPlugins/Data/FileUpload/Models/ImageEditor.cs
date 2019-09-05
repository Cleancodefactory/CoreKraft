using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Data.FileUpload.BaseClasses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System;
using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileUpload.Models
{
    internal class ImageEditor : IImageEditor
    {
        private readonly IPostedFile _Postedfile;

        private Image<Rgba32> _Image;
        private bool _IsDisposed = false;

        public ImageEditor(IPostedFile postedFile)
        {
            _Postedfile = postedFile ?? throw new ArgumentNullException($"{nameof(IPostedFile)} cannot be null.");

            _Image = Image.Load(postedFile.OpenReadStream());
            _Image.Mutate(i => i.AutoOrient());
        }

        public void Dispose()
        {
            if (!_IsDisposed)
            {
                _Image.Dispose();
                _Image = null;
                GC.SuppressFinalize(this);

                _IsDisposed = true;
            }
        }

        public void Resize(double width, double height)
        {
            if (width <= 0D)
            {
                throw new ArgumentNullException("Width cannot be less or equal to zero.");
            }

            if (height <= 0D)
            {
                throw new ArgumentNullException("Height cannot be less or equal to zero.");
            }

            double imageHeight = _Image.Height;
            double imageWidth = _Image.Width;
            double ratio = -1D;

            if (imageWidth >= imageHeight)
            {
                if (imageWidth > width)
                {
                    ratio = imageWidth / width;
                }
                else if (imageHeight > height)
                {
                    ratio = imageHeight / height;
                }
            }
            else
            {
                if (imageHeight > height)
                {
                    ratio = imageHeight / height;
                }
                else if (imageWidth > width)
                {
                    ratio = imageWidth / width;
                }
            }

            if (ratio > 0D)
            {
                _Image.Mutate(i => i.Resize((int)(imageWidth / ratio), (int)(imageHeight / ratio)));
            }
        }

        public void Save(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("Path cannot be null or white space.");
            }

            _Image.Save(Path.Combine(path));
        }
    }
}