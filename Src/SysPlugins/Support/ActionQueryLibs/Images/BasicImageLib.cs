using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

#nullable enable

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Images
{
    /// <summary>
    /// TODO: We need to be able to dispose some of the artifacts here. It should be possible to live without it, but it will take care for the memory better
    /// </summary>
    /// <typeparam name="HostInterface"></typeparam>
    [Library("basicimage", LibraryContextFlags.MainNode)]
    public class BasicImageLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private readonly object _LockObject = new object();
        private static FontCollection? _FontCollection;

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
                case nameof(ThumbImage):
                    return ThumbImage;
                case nameof(LimitSizeImage):
                    return LimitSizeImage;
                case nameof(WaterMarkImage):
                    return WaterMarkImage;
                case nameof(WaterMarkImageWithLogo):
                    return WaterMarkImageWithLogo;
                case nameof(GetFont):
                    return GetFont;
                case nameof(SaveImage):
                    return SaveImage;
                case nameof(LoadImage):
                    return LoadImage;
                case nameof(ImageHeight):
                    return ImageHeight;
                case nameof(ImageWidth):
                    return ImageWidth;
                case nameof(ImageLatitude):
                    return ImageLatitude;
                case nameof(ImageLongitude):
                    return ImageLongitude;
                case nameof(IsImage):
                    return IsImage;
                case nameof(JpegFromImage):
                    return JpegFromImage;
                case nameof(GifFromImage):
                    return GifFromImage;
                case nameof(PngFromImage):
                    return PngFromImage;
            }
            return null!;
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
        [Function(nameof(CreateImage), "")]
        public ParameterResolverValue CreateImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("Image - not enough arguments.");
            var file = args[0].Value as IPostedFile;
            if (file != null)
            {
                using var strm = file.OpenReadStream();
                try
                {
                    var image = Image.Load(strm);
                    // If you want to auto rotate the image by EXIF information with ImageSharp, use the following:
                    image.Mutate(img => img.AutoOrient());
                    lock (_LockObject)
                    {
                        _disposables.Add(image);
                    }
                    return new ParameterResolverValue(image);
                }
                catch (Exception)
                {
                    //
                }
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(DisposeImage), "")]
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

        [Function(nameof(ResizeImage), "")]
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

        [Function(nameof(LimitSizeImage), "Resizing the image by keeping the aspect ratio if any side is bigger than the specified size")]
        public ParameterResolverValue LimitSizeImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 2) throw new ArgumentException("LimitSizeImage requires 2 arguments");
            var image = args[0].Value as Image;
            if (image != null)
            {
                var size = Convert.ToInt32(args[1].Value);
                var width = 0;
                var height = 0;
                if (size < 32 || size > 2048) size = 256;
                if (image.Width >= image.Height)
                {
                    if (image.Width < size)
                    {
                        return new ParameterResolverValue(image);
                    }
                    width = size;
                    height = size * image.Height / image.Width;
                }
                else
                {
                    if (image.Height < size)
                    {
                        return new ParameterResolverValue(image);
                    }
                    height = size;
                    width = size * image.Width / image.Height;
                }
                if (width > 0 && height > 0)
                {
                    image.Mutate(pc => pc.Resize(width, height));
                    return new ParameterResolverValue(image);
                }
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(ThumbImage), "")]
        public ParameterResolverValue ThumbImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 2) throw new ArgumentException("ThumbImage requires 2 arguments");
            var image = args[0].Value as Image;
            if (image != null)
            {
                var size = Convert.ToInt32(args[1].Value);
                var width = 0;
                var height = 0;
                if (size < 32 || size > 2048) size = 256;
                if (image.Width >= image.Height)
                {
                    width = size;
                    height = size * image.Height / image.Width;
                }
                else
                {
                    height = size;
                    width = size * image.Width / image.Height;
                }
                if (width > 0 && height > 0)
                {
                    image.Mutate(pc => pc.Resize(width, height));
                    return new ParameterResolverValue(image);
                }
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(WaterMarkImage), "Updates an image with a text as watermark")]
        public ParameterResolverValue WaterMarkImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 6) throw new ArgumentException("WaterMarkImage requires 6 arguments");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                string drawText = Convert.ToString(args[1].Value) ?? string.Empty;
                Font? font = args[2].Value as Font;
                int penWidth = Convert.ToInt32(args[3].Value);
                byte brushTransparency = Convert.ToByte(args[4].Value);
                byte penTransparency = Convert.ToByte(args[5].Value);

                if (font == null)
                {
                    throw new ArgumentException("Font for WaterMarkImage is not provided.");
                }

                image = image.Clone(proc => ApplyScalingWaterMarkSimple(proc, font, drawText, penWidth, 10, brushTransparency, penTransparency));
                return new ParameterResolverValue(image);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(WaterMarkImageWithLogo), "Updates an image with another image as watermark")]
        public ParameterResolverValue WaterMarkImageWithLogo(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 3) throw new ArgumentException("WaterMarkImageWithLogo requires 3 arguments");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                Image? logo = args[1].Value as Image;
                if (logo == null)
                {
                    throw new ArgumentException("Logo for WaterMarkImageWithLogo doesn't contain valid image. Please create one with CreateImage(...)");
                }
                float alpha = (float)Convert.ToDouble(args[2].Value);
                if (alpha < 0 || alpha > 1)
                {
                    alpha = 1f;
                }
                image = image.Clone(ctx => ApplyScalingWaterMarkWithLogo(ctx, logo, 10, alpha));
                return new ParameterResolverValue(image);
            }
            return new ParameterResolverValue(null);

        }

        private IImageProcessingContext ApplyScalingWaterMarkWithLogo(IImageProcessingContext processingContext,
            Image logo,
            float padding,
            float alpha = 1f
          )
        {
            Size imgSize = processingContext.GetCurrentSize();

            int widthDifference = (int)(imgSize.Width - logo.Width);
            int newLogoWidth;
            if (widthDifference >= 0)   //increase logo size
            {
                newLogoWidth = Convert.ToInt32((logo.Size.Width + widthDifference) * 0.7);
            }
            else                        //decrease logo size
            {
                //It is not a bug: widthDifference comes negative and that's why again plus
                newLogoWidth = Convert.ToInt32((logo.Size.Width + widthDifference) * 0.7);
            }

            logo.Mutate(x => x
                .Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new Size() { Width = newLogoWidth }
                }));

            Point center = new SixLabors.ImageSharp.Point((int)padding, imgSize.Height / 2);
            int x = imgSize.Width / 2 - logo.Width / 2;
            int y = imgSize.Height / 2 - logo.Height / 2;
            return processingContext.DrawImage(logo, new Point(x, y), alpha);
        }

        [Function(nameof(GetFont), "Font family name: e.g. Open Sans ExtraBold, Rubik ")]
        public ParameterResolverValue GetFont(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 3) throw new ArgumentException("GetFont requires 3 arguments");
            var fontName = Convert.ToString(args[0].Value) ?? string.Empty;
            float size = (float)Convert.ToDouble(args[1].Value);
            var fontStyle = FontStyle.Regular;
            var fontStyleTemp = Convert.ToString(args[2].Value);
            if (!string.IsNullOrWhiteSpace(fontStyleTemp))
            {
                if (Enum.TryParse<FontStyle>(fontStyleTemp, true, out FontStyle f))
                {
                    fontStyle = f;
                }
            }
            FontFamily fontFamily;
            Font font;
            if (GetFonts().TryGet(fontName, out fontFamily))
            {
                font = fontFamily.CreateFont(size, fontStyle);
            }
            else
            {
                fontFamily = GetFonts().Families.FirstOrDefault();
                if (fontFamily.Equals(default))
                {
                    fontFamily = SystemFonts.Families.FirstOrDefault();
                }
                if (fontFamily.Equals(default))
                {
                    throw new InvalidOperationException("No fonts available.");
                }
                font = fontFamily.CreateFont(size, fontStyle);
            }
            return new ParameterResolverValue(font);
        }
        private FontCollection GetFonts()
        {
            if (_FontCollection == null)
            {
                _FontCollection = new FontCollection();
                var assembly = Assembly.GetExecutingAssembly();
                string[] names = assembly.GetManifestResourceNames();

                foreach (string fontName in names)
                {
                    //we accept only font files (.ttf in folder fonts)
                    string pattern = @".*\.fonts\..*\.ttf$";
                    RegexOptions options = RegexOptions.Singleline;
                    if (Regex.IsMatch(fontName, pattern, options))
                    {
                        using Stream? stream = assembly.GetManifestResourceStream(fontName);
                        if (stream != null)
                        {
                            _FontCollection.Add(stream);
                        }
                    }
                }
            }
            return _FontCollection!;
        }
        private IImageProcessingContext ApplyScalingWaterMarkSimple(IImageProcessingContext processingContext,
            Font font,
            string text,
            int penWidth,
            float padding,
            byte brushTransparency,
            byte penTransparency
          )
        {
            Size imgSize = processingContext.GetCurrentSize();

            float targetWidth = imgSize.Width - (padding * 2);
            float targetHeight = imgSize.Height - (padding * 2);

            // measure the text size
            FontRectangle size = TextMeasurer.MeasureSize(text, new TextOptions(font));

            //find out how much we need to scale the text to fill the space (up or down)
            float scalingFactor = Math.Min(targetWidth / size.Width, targetHeight / size.Height);

            //create a new font
            Font scaledFont = new Font(font, scalingFactor * font.Size);

            var center = new PointF(padding, imgSize.Height / 2);
            var drawingOptions = new DrawingOptions()
            {
                //TextOptions = {
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    VerticalAlignment = VerticalAlignment.Center
                //}
            };
            Rgba32 rgbaColorBrush = new Rgba32(0, 0, 0, brushTransparency);
            SolidBrush brush = Brushes.Solid(new Color(rgbaColorBrush));

            Rgba32 rgbaColorPen = new Rgba32(0, 0, 0, penTransparency);
            SolidPen pen = Pens.Solid(new Color(rgbaColorPen), penWidth);
            return processingContext.DrawText(drawingOptions, text, scaledFont, brush, pen, center);
        }

        [Function(nameof(SaveImage), "")]
        public ParameterResolverValue SaveImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 2) throw new ArgumentException("SaveImage requires at least 2 arguments image, savepath");
            var image = args[0].Value as Image;
            if (image != null)
            {
                string? path = args[1].Value as string;
                if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                {
                    throw new ArgumentException("The second parameter to SaveImage is not fully qualfied path");
                }
                image.Save(path);
                return new ParameterResolverValue(path);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(LoadImage), "")]
        public ParameterResolverValue LoadImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("LoadImage requires 1 argument");
            string? path = args[0].Value as string;
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("LoadImage has a null path argument");
            if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("LoadImage has a non-fully qualified path argument");
            var image = Image.Load(path);
            // If you want to auto rotate the image by EXIF information with ImageSharp, use the following:
            image.Mutate(img => img.AutoOrient());
            lock (_LockObject)
            {
                _disposables.Add(image);
            }
            return new ParameterResolverValue(image);
        }

        [Function(nameof(ImageHeight), "")]
        public ParameterResolverValue ImageHeight(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageHeight requires one argument - the image");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                return new ParameterResolverValue(image.Height);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(ImageWidth), "")]
        public ParameterResolverValue ImageWidth(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageWidth requires one argument - the image");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                return new ParameterResolverValue(image.Width);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(ImageLongitude), "Extracts longitude from image's EXIF metadata, converting the DMS (degrees/minutes/seconds) rationals into decimal degrees.")]
        public ParameterResolverValue ImageLongitude(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageLongitude requires one argument - the image");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                ExifProfile? exif = image.Metadata.ExifProfile;
                if (exif is null)
                {
                    return new ParameterResolverValue(null);
                }

                // Extract GPS metadata using TryGetValue
                if (!exif.TryGetValue(ExifTag.GPSLongitudeRef, out IExifValue<string>? lonRefValue) ||
                    !exif.TryGetValue(ExifTag.GPSLongitude, out IExifValue<Rational[]>? lonDmsValue))
                {
                    return new ParameterResolverValue(null);
                }

                string? lonRef = lonRefValue.Value;
                Rational[]? lonDms = lonDmsValue.Value;
                if (lonRef == null || lonDms == null)
                {
                    return new ParameterResolverValue(null);
                }

                double longitude = DmsToDecimalDegrees(lonDms, lonRef);
                return new ParameterResolverValue(longitude);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(ImageLatitude), "Extracts latitude from image's EXIF metadata, converting the DMS (degrees/minutes/seconds) rationals into decimal degrees.")]
        public ParameterResolverValue ImageLatitude(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ImageLatitude requires one argument - the image");
            Image? image = args[0].Value as Image;
            if (image != null)
            {
                ExifProfile? exif = image.Metadata.ExifProfile;
                if (exif is null)
                {
                    return new ParameterResolverValue(null);
                }

                // Extract GPS metadata using TryGetValue
                if (!exif.TryGetValue(ExifTag.GPSLatitudeRef, out IExifValue<string>? latRefValue) ||
                    !exif.TryGetValue(ExifTag.GPSLatitude, out IExifValue<Rational[]>? latDmsValue))
                {
                    return new ParameterResolverValue(null);
                }

                string? latRef = latRefValue.Value;
                Rational[]? latDms = latDmsValue.Value;
                if (latRef == null || latDms == null)
                {
                    return new ParameterResolverValue(null);
                }

                double latitude = DmsToDecimalDegrees(latDms, latRef);
                return new ParameterResolverValue(latitude);
            }
            return new ParameterResolverValue(null);
        }

        private static double DmsToDecimalDegrees(Rational[] dms, string refStr)
        {
            if (dms.Length < 3)
            {
                throw new ArgumentException("DMS array must have three rationals (deg, min, sec).");
            }

            double deg = RationalToDouble(dms[0]);
            double min = RationalToDouble(dms[1]);
            double sec = RationalToDouble(dms[2]);

            double value = deg + (min / 60.0) + (sec / 3600.0);

            if (string.Equals(refStr, "S", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(refStr, "W", StringComparison.OrdinalIgnoreCase))
            {
                value = -value;
            }

            return value;
        }

        private static double RationalToDouble(Rational r)
            => r.Numerator / (double)r.Denominator;

        [Function(nameof(IsImage), "")]
        public ParameterResolverValue IsImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsImage requires one argument - the image");
            Image? image = args[0].Value as Image;
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
        private IPostedFile? _PostedFileFromImage(Image? image, string astype, string? dispFilename)
        {
            if (image != null)
            {
                string name = "unnamed";
                if (!string.IsNullOrWhiteSpace(dispFilename))
                {
                    name = dispFilename;
                }
                Action<Image, Stream>? proc = null;
                string? ct = null;
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
                if (proc != null && ct != null)
                {
                    var ms = new MemoryStream();
                    proc(image, ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var pf = new PostedFile(ct, ms.Length, name, name, m =>
                    {
                        if (m is byte[] buffer)
                        {
                            return new MemoryStream(buffer, 0, buffer.Length);
                        }
                        return new MemoryStream(Array.Empty<byte>());
                    }, ms.GetBuffer());
                    return pf;
                }
            }
            return null;
        }
        private ParameterResolverValue __PostedFileFromImage(ParameterResolverValue[] args, string _type)
        {
            Image? image = null;
            string type = _type ?? "jpeg";
            string? name = null;
            if (args.Length >= 1) image = args[0].Value as Image;
            if (args.Length >= 2) name = Convert.ToString(args[1].Value);
            return new ParameterResolverValue(_PostedFileFromImage(image, type, name));
        }

        [Function(nameof(JpegFromImage), "")]
        public ParameterResolverValue JpegFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("JpegFromImage has too few arguments");
            return __PostedFileFromImage(args, "jpeg");
        }

        [Function(nameof(GifFromImage), "")]
        public ParameterResolverValue GifFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("GifFromImage has too few arguments");
            return __PostedFileFromImage(args, "gif");
        }

        [Function(nameof(PngFromImage), "")]
        public ParameterResolverValue PngFromImage(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("GifFromImage has too few arguments");
            return __PostedFileFromImage(args, "png");
        }
    }
}
