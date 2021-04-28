using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.IO;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Images
{
    /// <summary>
    /// TODO: We need to be able to dispose some of the artifacts here. It should be possible to live without it, but it will take care for the memory better
    /// </summary>
    /// <typeparam name="HostInterface"></typeparam>
    public class BasicImageLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(CreateImage):
                    return CreateImage;
                case nameof(DisposeImage):
                    return DisposeImage;
                case nameof(ResizeImage):
                    return ResizeImage;
                case nameof(SaveImage):
                    return SaveImage;
                case nameof(LoadImage):
                    return LoadImage;
                case nameof(ImageHeight):
                    return ImageHeight;
                case nameof(ImageWidth):
                    return ImageWidth;
                case nameof(IsImage):
                    return IsImage;
                case nameof(JpegFromImage):
                    return JpegFromImage;
                case nameof(GifFromImage):
                    return GifFromImage;
                case nameof(PngFromImage):
                    return PngFromImage;
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Basic Image library (no symbols)", null);
        }

        private List<object> _disposables = new List<object>();
        public void ClearDisposables()
        {
            lock (_LockObject)
            {
                for (int i = 0; i < _disposables.Count; i++)
                {
                    if (_disposables[i] is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                }
                _disposables.Clear();
            }
        }
        #endregion

        /// <summary>
        /// Creates an image from IPostedFile
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue CreateImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("Image - not enough arguments.");
            var file = args[0].Value as IPostedFile;
            if (file != null)
            {

                var strm = file.OpenReadStream();
                var image = Image.Load(strm);
                lock (_LockObject)
                {
                    _disposables.Add(image);
                }
                strm.Close();
                return new ParameterResolverValue(image);

            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue DisposeImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            int x;
            lock (_LockObject)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Value != null)
                    {
                        x = _disposables.IndexOf(args[i].Value);
                        if (x >= 0)
                        {
                            if (_disposables[x] is IDisposable disp)
                            {
                                disp.Dispose();
                            }
                            _disposables.RemoveAt(x);
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue ResizeImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 3) throw new ArgumentException("ResizeImage requires at least 3 arguments");
            var image = args[0].Value as Image;
            if (image != null)
            {
                /*
                IResampler resampler = null;
                if (args.Length > 3)
                {
                    string resamplerName = args[3].Value as string;
                    if (resamplerName != null)
                    {

                    }
                }
                */
                var width = Convert.ToInt32(args[1].Value);
                var height = Convert.ToInt32(args[2].Value);
                if (width > 0 && height > 0)
                {
                    image.Mutate(pc => pc.Resize(width, height));
                }
                return new ParameterResolverValue(image);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue SaveImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 2) throw new ArgumentException("SaveImage requires at least 2 arguments image, savepath");
            var image = args[0].Value as Image;
            if (image != null)
            {
                string path = args[1].Value as string;
                if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("The second parameter to SaveImage is not fully qualfied path");
                image.Save(path);
                return new ParameterResolverValue(path);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue LoadImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("LoadImage requires 1 argument");
            string path = args[0].Value as string;
            if (path == null) throw new ArgumentException("LoadImage has a null path argument");
            if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("LoadImage has a non-fully qualified path argument");
            var image = Image.Load(path);
            lock (_LockObject)
            {
                _disposables.Add(image);
            }
            return new ParameterResolverValue(image);
        }
        public ParameterResolverValue ImageHeight(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageHeight requires one argument - the image");
            Image image = args[0].Value as Image;
            if (image != null)
            {
                return new ParameterResolverValue(image.Height);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue ImageWidth(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageWidth requires one argument - the image");
            Image image = args[0].Value as Image;
            if (image != null)
            {
                return new ParameterResolverValue(image.Width);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue IsImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsImage requires one argument - the image");
            Image image = args[0].Value as Image;
            if (image != null)
            {
                return new ParameterResolverValue(true);
            }
            return new ParameterResolverValue(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private IPostedFile _PostedFileFromImage(Image image, string astype, string dispFilename)
        {
            if (image != null)
            {
                string name = "unnamed";
                if (!string.IsNullOrWhiteSpace(dispFilename))
                {
                    name = dispFilename;
                }
                Action<Image, Stream> proc = null;
                string ct = null;
                if (astype == "jpeg" || astype == "jpg")
                {
                    proc = ImageExtensions.SaveAsJpeg;
                    ct = "image/jpeg";
                    name = name + ".jpg";
                } 
                else if (astype == "gif")
                {
                    proc = ImageExtensions.SaveAsGif;
                    ct = "image/gif";
                    name = name + ".gif";
                } 
                else if (astype == "png")
                {
                    proc = ImageExtensions.SaveAsPng;
                    ct = "image/png";
                    name = name + ".png";
                }
                else
                {
                    throw new ArgumentException("The image type is not recognized.");
                }
                if (proc != null)
                {
                    var ms = new MemoryStream();
                    proc(image, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var pf = new PostedFile(ct, ms.Length, name, name, m => m as Stream, ms);
                    return pf;
                }
            }
            return null;
        }
        private ParameterResolverValue __PostedFileFromImage(ParameterResolverValue[] args, string _type)
        {
            Image image = null;
            string type = _type ?? "jpeg";
            string name = null;
            if (args.Length >= 1) image = args[0].Value as Image;
            if (args.Length >= 2) name = Convert.ToString(args[1].Value);
            return new ParameterResolverValue(_PostedFileFromImage(image, type, name));
        }
        public ParameterResolverValue JpegFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("JpegFromImage has too few arguments");
            return __PostedFileFromImage(args, "jpeg");
        }
        public ParameterResolverValue GifFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("GifFromImage has too few arguments");
            return __PostedFileFromImage(args, "gif");
        }
        public ParameterResolverValue PngFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("GifFromImage has too few arguments");
            return __PostedFileFromImage(args,"png");
        }
    }
}
