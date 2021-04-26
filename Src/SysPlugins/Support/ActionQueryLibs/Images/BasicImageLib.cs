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
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Default library (no symbols)", null);
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
    }
}
