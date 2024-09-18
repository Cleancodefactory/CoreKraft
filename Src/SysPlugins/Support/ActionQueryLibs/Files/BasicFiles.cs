using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Utilities;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Files
{
    [Library("files", LibraryContextFlags.MainNode)]
    public class BasicFiles<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {

        private readonly object _LockObject = new Object();

        public BasicFiles()
        {
            //_disposables.Add(http);
        }
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(IsPostedFile): return IsPostedFile;
                case nameof(PostedFileSize): return PostedFileSize;
                case nameof(PostedFileName): return PostedFileName;
                case nameof(PostedFileType): return PostedFileType;
                case nameof(CombinePaths): return CombinePaths;
                case nameof(DeleteFile): return DeleteFile;
                case nameof(SaveFile): return SaveFile;
                case nameof(PrependFileName): return PrependFileName;
                case nameof(CreateDirectory): return CreateDirectory;
                case nameof(ForkFile): return ForkFile;
                case nameof(FileMD5): return FileMD5;
                case nameof(SaveFileToSpread): return SaveFileToSpread;
                case nameof(PostedFile): return this.PostedFile;
                case nameof(FileResponse): return FileResponse;
                case nameof(FileExists): return FileExists;
                case nameof(ReadFileAsText): return ReadFileAsText;
                case nameof(UseUnixPathResults):return UseUnixPathResults;
                    // case nameof(CalcSpreadName): return CalcSpreadName;//TODO Under consideration
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Basic File library (no symbols)", null);
        }

        private readonly List<object> _disposables = new List<object>();
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

        #region Session data
        public bool UnixPathResults = true; // By default true
        #endregion  

        #region Functions
        [Function(nameof(UseUnixPathResults),"Gets/sets the mode for returned paths - ture=unix slashes, false=local OS decided slashes")]
        [Parameter(0,"mode", "Optional sets mode to: ture = unix slashes, false = local OS decided slashes",TypeFlags.Bool | TypeFlags.Optional)]
        [Result("The current mode", TypeFlags.Bool)]
        public ParameterResolverValue UseUnixPathResults(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length > 0) {
                UnixPathResults = Convert.ToBoolean(args[0].Value);
            }
            return new ParameterResolverValue(UnixPathResults);
        }

        [Function(nameof(IsPostedFile), "Check if the argument is a PostedFile object")]
        [Parameter(0, "postedfile", "a value to check", TypeFlags.Varying)]
        [Result("true if the argument is a PostedFile and false otherwise", TypeFlags.Bool)]
        public ParameterResolverValue IsPostedFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsFile accepts single argument.");
            return new ParameterResolverValue(args[0].Value is IPostedFile);
        }

        [Function(nameof(PostedFileSize), "")]
        public ParameterResolverValue PostedFileSize(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileSize requires single argument");
            IPostedFile pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileSize argument is not a posted file");
            return new ParameterResolverValue((double)pf.Length);
        }

        [Function(nameof(PostedFileName), "")]
        public ParameterResolverValue PostedFileName(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileName requires single argument");
            var pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileName argument is not a posted file");
            return new ParameterResolverValue(pf.FileName);
        }

        [Function(nameof(PostedFileType), "")]
        public ParameterResolverValue PostedFileType(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileType requires single argument");
            var pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileType argument is not a posted file");
            return new ParameterResolverValue(pf.ContentType);
        }

        [Function(nameof(CombinePaths), "")]
        public ParameterResolverValue CombinePaths(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("CombinePaths accepts two arguments.");
            var path = args[0].Value as string;
            var file = args[1].Value as string;
            if (path == null || file == null) throw new ArgumentException("CombinePaths one or both of the parameters cannot convert to string.");
            path = ModeArgument(args[0]);
            file = ModeArgument(args[1]);
            if (Path.IsPathFullyQualified(file)) throw new ArgumentException("CombinePaths allows only partial paths for the second argument");
            return ApplySlashes(Path.Combine(path, file));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">(IPostedFile pf, string savepath)</param>
        /// <returns></returns>
        [Function(nameof(SaveFile), "")]
        [Parameter(0, "postedfile", "The PostedFile object to save to a physical file in the FS", TypeFlags.PostedFile)]
        [Parameter(1, "savepath", "Fully qualified path of the file to save.", TypeFlags.String)]
        public ParameterResolverValue SaveFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Save file takes two arguments: file, save_path");
            IPostedFile file = args[0].Value as IPostedFile;
            string savepath = args[1].Value as string;
            if (savepath == null || file == null) throw new ArgumentException("SaveFile: either file, path or both are null");
            savepath = ModeArgument(args[1]);
            if (!Path.IsPathFullyQualified(savepath)) throw new ArgumentException("SaveFile the specified path is not fully qualified. Did you forget to combine paths?");
            using var stream = new FileStream(savepath, FileMode.Create);
            using var source = file.OpenReadStream();
            source.CopyTo(stream);

            var scope = Scope(ctx);
            if (scope is IFileTransactionSupport fts)
            {
                fts.DeleteOnRollback(savepath);
            }

            return new ParameterResolverValue(true);
        }

        [Function(nameof(DeleteFile), "")]
        public ParameterResolverValue DeleteFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("DeleteFile takes single argument: filepath");
            string savepath = args[0].Value as string;
            if (savepath == null) throw new ArgumentException("DeleteFile: filepath is null or not a string");
            savepath = ModeArgument(args[0]);
            if (!Path.IsPathFullyQualified(savepath)) throw new ArgumentException("DeleteFile the specified path is not fully qualified. Did you forget to combine paths?");

            if (File.Exists(savepath))
            {
                var scope = Scope(ctx);
                if (scope is IFileTransactionSupport fts)
                {
                    fts.DeleteOnCommit(savepath);
                }
                else
                {
                    File.Delete(savepath);
                }
            }
            return new ParameterResolverValue(true);
        }

        [Function(nameof(PrependFileName), "")]
        public ParameterResolverValue PrependFileName(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("PrependFileName requires two arguments (prefix, filename)");
            if (args[0].Value == null) throw new ArgumentException("PrependFileName prefix argument is null");
            var prefix = args[0].Value.ToString();
            var filename = args[1].Value as string;
            if (filename != null) filename = ModeArgument(args[1]);
            if (filename == null) throw new ArgumentException("PrependFileName filename is null or not a string");

            var name = $"{prefix}-{Path.GetFileNameWithoutExtension(filename)}{Path.GetExtension(filename)}";
            return new ParameterResolverValue(name);
        }

        [Function(nameof(CreateDirectory), "")]
        public ParameterResolverValue CreateDirectory(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("CreateDirectory requires single parameter");
            var path = args[0].Value as string;
            if (path != null) path = ModeArgument(args[0]);
            if (path == null) throw new ArgumentNullException("CreateDirectory argument is null or not a string");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return ApplySlashes(path);
        }

        // TODO Consider the need of this
        public ParameterResolverValue CalcSpreadName(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 4) throw new ArgumentException("CalcSpreadName requires 4 arguments (sring basedir, int spread, long id, postedfile)");
            string basedir;
            int spread;
            long id;
            IPostedFile file;
            try
            {
                basedir = args[0].Value as string;
                if (basedir != null) basedir = ModeArgument(args[0]);
                spread = Convert.ToInt32(args[1].Value);
                id = Convert.ToInt64(args[2].Value);
                file = args[3].Value as IPostedFile;
                if (file == null) throw new ArgumentException("There is no a posted file");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("CalcSpreadName has some invalid arguments", ex);
            }
            if (!Directory.Exists(basedir))
            {
                Directory.CreateDirectory(basedir);
            }
            string subdir = (id % spread).ToString();
            string filedir = Path.Combine(basedir, subdir);
            if (!Directory.Exists(filedir))
            {
                Directory.CreateDirectory(filedir);
            }
            string filespreaddir = Path.Combine(subdir, $"{id}-{file.FileName}");
            _ = Path.Combine(basedir, filespreaddir);

            return ApplySlashes(filespreaddir);
        }

        /// <summary>
        /// File names will look like 345-somename.jpg
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">(sring basedir, int spread, long id, IPostedFile file)</param>
        /// <returns></returns>
        [Function(nameof(SaveFileToSpread), "")]
        public ParameterResolverValue SaveFileToSpread(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 4) throw new ArgumentException("SaveFileToSpread requires 4 arguments (sring basedir, int spread, long id, string filename)");
            string basedir;
            int spread;
            long id;
            IPostedFile file;
            try
            {
                basedir = args[0].Value as string;
                if (basedir != null) basedir = ModeArgument(args[0]);
                spread = Convert.ToInt32(args[1].Value);
                id = Convert.ToInt64(args[2].Value);
                file = args[3].Value as IPostedFile;
                if (file == null) throw new ArgumentException("filename is not a posted file");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("SaveFileToSpread has some invalid arguments", ex);
            }
            if (!Directory.Exists(basedir))
            {
                Directory.CreateDirectory(basedir);
            }
            string subdir = (id % spread).ToString();
            string filedir = Path.Combine(basedir, subdir);
            if (!Directory.Exists(filedir))
            {
                Directory.CreateDirectory(filedir);
            }
            string filespreaddir = Path.Combine(subdir, $"{id}-{file.FileName}");
            Regex regex = new Regex(@"\.\.+", RegexOptions.Singleline);
            filespreaddir = regex.Replace(filespreaddir, ".");
            string filefullpath = Path.Combine(basedir, filespreaddir);
            // Saving
            using var stream = new FileStream(filefullpath, FileMode.Create);
            using var strm = file.OpenReadStream();
            strm.CopyTo(stream);

            var scope = Scope(ctx);
            if (scope is IFileTransactionSupport fts)
            {
                fts.DeleteOnRollback(filefullpath);
            }
            return ApplySlashes(filespreaddir);
        }

        [Function(nameof(ForkFile), "Clones the posted file into two PostedFiles in a List, the original is disposed")]
        [Parameter(0, "postedfile", "Posteed file to fork", TypeFlags.PostedFile)]
        [Parameter(1, "forkparts", "In how many parts the file should be forked to. If not supplied the Postedfile will be forked to 2.", TypeFlags.Int)]
        [Result("List with 2 or if the second parameter supplied into X forked Posted files (as memory streams)", TypeFlags.List)]
        public ParameterResolverValue ForkFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                throw new ArgumentException("ForkFile accepts 1 or 2 arguments (PostedFile to clone or the count of parts it should be forked to.)");
            }
            int partsCount = 2;
            if (args.Length == 2)
            {
                if (!int.TryParse(args[1].Value?.ToString(), out partsCount))
                {
                    throw new ArgumentException("Second parameter should be of type integer");
                }
            }
            if (args[0].Value is IPostedFile pf)
            {
                using var strm = pf.OpenReadStream();
                List<ParameterResolverValue> list = new List<ParameterResolverValue>(partsCount);

                MemoryStream memoryStreamTemp = new MemoryStream();
                strm.CopyTo(memoryStreamTemp);

                for (int i = 0; i < partsCount; i++)
                {
                    MemoryStream memoryStream = new MemoryStream();

                    memoryStreamTemp.Position = 0;
                    memoryStreamTemp.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    list.Add(new ParameterResolverValue(new PostedFile(pf.ContentType, memoryStream.Length, pf.Name, pf.FileName, m => m as Stream, memoryStream)));
                }

                return new ParameterResolverValue(list);
            }
            else
            {
                throw new ArgumentException("PostedFile expected!");
            }
        }

        [Function(nameof(FileMD5), "Creates MD5 hash from PostedFile")]
        [Parameter(0, "postedfile", "PostedFile to get MD5 hash from", TypeFlags.PostedFile)]
        [Result("Returns the calculated hash as string", TypeFlags.String)]
        public ParameterResolverValue FileMD5(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("FileMD5 accepts 1 argument of type PostedFile");
            if (args[0].Value is IPostedFile pf)
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] hash = md5.ComputeHash(pf.OpenReadStream());
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return new ParameterResolverValue(sb.ToString());
            }
            else
            {
                throw new ArgumentException("The passed in argument is not of type PostedFile");
            }
        }

        [Function(nameof(ReadFileAsText), "Reads content as text from PostedFile")]
        [Parameter(0, "postedfile", "PostedFile to get path from", TypeFlags.PostedFile)]
        [Result("Returns the content as string", TypeFlags.String)]
        public ParameterResolverValue ReadFileAsText(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("ReadFileAsText accepts 1 argument of type PostedFile");
            if (args[0].Value is IPostedFile pf)
            {
                StreamReader sr = new StreamReader(pf.OpenReadStream());                
                return new ParameterResolverValue(sr.ReadToEnd());
            }
            else
            {
                throw new ArgumentException("The passed in argument is not of type PostedFile");
            }
        }

        /// <summary>
        /// Creates a PostedFile from a disk file.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">(string filepath, string contentType)</param>
        /// <returns></returns>
        [Function(nameof(PostedFile), "")]
        [Parameter(0, "filepath", "File to encompass", TypeFlags.String)]
        [Parameter(1, "contentType", "Assumed content type, if omitted attempt to deduce it will be made.", TypeFlags.String | TypeFlags.Optional)]
        [Result("The created PostedFile object", TypeFlags.PostedFile)]
        public ParameterResolverValue PostedFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("PostedFile accepts 1 or 2 arguments (filepath[, contentType])");
            var filepath = args[0].Value as string;
            if (filepath != null) filepath = ModeArgument(args[0]);
            string contentType = null;
            if (args.Length > 1)
            {
                contentType = args[1].Value as string; // TODO: Calc if missing
            }
            if (contentType == null)
            {
                if (!(new FileExtensionContentTypeProvider().TryGetContentType(filepath, out contentType)))
                {
                    contentType = "application/octet-stream";
                };
            }

            if (string.IsNullOrWhiteSpace(filepath))
            {
                IPostedFile posted = args[0].Value as IPostedFile;
                if (posted != null)
                {
                    string name = null, fileName = null;
                    if (args.Length > 2) name = args[2].Value as string;
                    if (args.Length > 3) fileName = args[3].Value as string;
                    return new ParameterResolverValue(new PostedFile(posted, contentType, name, fileName));
                }
                else
                {
                    throw new ArgumentException("PostedFile - forst argument must be filepath or another PostedFile and it is not!");
                }
            }
            else
            {
                if (!File.Exists(filepath)) throw new Exception($"PostedFile: {filepath} does not exist");
                var fi = new FileInfo(filepath);

                var pf = new PostedFile(contentType, fi.Length, Path.GetFileName(filepath), filepath, path =>
                {
                    return File.Open(path as string, FileMode.Open, FileAccess.Read, FileShare.Read);
                }, filepath);
                return new ParameterResolverValue(pf);
            }
            //throw new ArgumentException("PostedFile - filepath is empty or null");
        }

        [Function(nameof(FileResponse), "")]
        public ParameterResolverValue FileResponse(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("FileResponse accepts 1 argument (PostedFile)");
            var pf = args[0].Value as IPostedFile;
            //if (pf == null) throw new ArgumentException("FileResponse - argument is not IPostedFile. Use FileResponse(PostedFile(...))");
            if (ctx is IDataLoaderContext ctx1)
            {
                ctx1.ProcessingContext.ReturnModel.BinaryData = pf;
                ctx1.ProcessingContext.ReturnModel.ResponseBuilder = new BinaryResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx1.ProcessingContext }));
            }
            else if (ctx is INodePluginContext ctx2)
            {
                ctx2.ProcessingContext.ReturnModel.BinaryData = pf;
                ctx2.ProcessingContext.ReturnModel.ResponseBuilder = new BinaryResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx2.ProcessingContext }));
            }
            return new ParameterResolverValue(pf);
        }

        [Function(nameof(FileExists), "")]
        public ParameterResolverValue FileExists(HostInterface ctx, ParameterResolverValue[] args)
        {
            
            if (args.Length != 1) throw new ArgumentException("FileExists requies one argument - the path to the file");
            var path = ModeArgument(args[0]);

            return new ParameterResolverValue(File.Exists(path));
        }


        #endregion


        #region Internal helpers
        private ParameterResolverValue ApplySlashes(string str) {
            if (string.IsNullOrWhiteSpace(str)) return new ParameterResolverValue(str);
            if (UnixPathResults) {
                return new ParameterResolverValue(str.Replace("\\", "/"));
            } else {
                return new ParameterResolverValue(str);
            }
        }
        private string ModeArgument(ParameterResolverValue v) {
            var str = Convert.ToString(v.Value);
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (UnixPathResults) {
                return str.Replace("\\", "/");
            } else {
                return str;
            }
        }
        private static IPluginsSynchronizeContextScoped Scope(HostInterface ctx)
        {
            if (ctx is IDataLoaderContext ldrctx)
            {
                return ldrctx.OwnContextScoped;
            }
            else if (ctx is INodePluginContext ndctx)
            {
                return ndctx.OwnContextScoped;
            }
            return null;
        }
        #endregion

    }
}
