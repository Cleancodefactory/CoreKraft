using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Utilities;
using Ccf.Ck.Models.ContextBasket;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.SysPlugins.Data.FileUploadMng
{
    /// <summary>
    /// Query syntax
    /// 
    ///   Return(filename)
    ///   ReturnPreview(filename)
    ///   Store(filename, file)
    /// </summary>
    public class FileUploadMngImp : DataLoaderBase<FileUploadMngSynchronizeContextScopedImp>
    {
        public FileUploadMngImp()
        {
        }

        protected override void ExecuteRead(IDataLoaderReadContext execContext)
        {
            string qry = GetQuery(execContext);
            if (qry != null)
            {
                var runner = Compiler.Compile(qry);
                if (runner.ErrorText != null)
                {
                    throw new Exception(runner.ErrorText);
                }
                var host = new ActionQueryHost<IDataLoaderReadContext>(execContext)
                {
                    { nameof(IsPostedFile), IsPostedFile },
                    { nameof(PostedFileSize), PostedFileSize },
                    { nameof(PostedFileName), PostedFileName },
                    { nameof(PostedFileType), PostedFileType },
                    { nameof (CombinePaths), CombinePaths },
                    { nameof(DeleteFile), DeleteFile },
                    { nameof(SaveFile), SaveFile },
                    { nameof(PrependFileName), PrependFileName },
                    { nameof(CreateDirectory), CreateDirectory },
                    { nameof(SaveFileToSpread), SaveFileToSpread }
                };

                var result = runner.ExecuteScalar(host);
            }
            // Else nothing to do.
        }

        protected override void ExecuteWrite(IDataLoaderWriteContext execContext)
        {
            string qry = GetQuery(execContext);
            if (qry != null)
            {
                var runner = Compiler.Compile(qry);
                if (runner.ErrorText != null)
                {
                    throw new Exception(runner.ErrorText);
                }
                var host = new ActionQueryHost<IDataLoaderWriteContext>(execContext)
                {
                    { nameof(IsPostedFile), IsPostedFile },
                    { nameof(PostedFileSize), PostedFileSize },
                    { nameof(PostedFileName), PostedFileName },
                    { nameof(PostedFileType), PostedFileType },
                    { nameof (CombinePaths), CombinePaths },
                    { nameof(DeleteFile), DeleteFile },
                    { nameof(SaveFile), SaveFile },
                    { nameof(PrependFileName), PrependFileName },
                    { nameof(CreateDirectory), CreateDirectory },
                    { nameof(SaveFileToSpread), SaveFileToSpread }
                };

                var result = runner.ExecuteScalar(host);
            }
            // Else nothing to do.
        }

        #region Helpers
        
        private string GetQuery(IDataLoaderContext ctx)
        {
            if (ctx.Action == ModelConstants.ACTION_READ)
            {
                var op = ctx.CurrentNode?.Read?.Select;
                if (op != null)
                {
                    return op.Query;
                }
            } 
            else if (ctx.Action == ModelConstants.ACTION_WRITE)
            {
                switch (ctx.Operation)
                {
                    case ModelConstants.OPERATION_INSERT:
                        return ctx.CurrentNode?.Write?.Insert?.Query;
                    case ModelConstants.OPERATION_UPDATE:
                        return ctx.CurrentNode?.Write?.Update?.Query;
                    case ModelConstants.OPERATION_DELETE:
                        return ctx.CurrentNode?.Write?.Delete?.Query;
                }
            }
            return null;
        }
        #endregion

        #region Available functions - usable in the query
        public ParameterResolverValue IsPostedFile(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsFile accepts single argument.");
            return new ParameterResolverValue(args[0].Value is IPostedFile);
        }
        public ParameterResolverValue PostedFileSize(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileSize requires single argument");
            var pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileSize argument is not a posted file");
            return new ParameterResolverValue((double)pf.Length);
        }
        public ParameterResolverValue PostedFileName(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileSize requires single argument");
            var pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileSize argument is not a posted file");
            return new ParameterResolverValue(pf.FileName);
        }
        public ParameterResolverValue PostedFileType(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("PostedFileSize requires single argument");
            var pf = args[0].Value as IPostedFile;
            if (pf == null) throw new ArgumentException("PostedFileSize argument is not a posted file");
            return new ParameterResolverValue(pf.ContentType);
        }
        
        public ParameterResolverValue CombinePaths(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("CombinePaths accepts two arguments.");
            var path = args[0].Value as string;
            var file = args[1].Value as string;
            if (path == null || file == null) throw new ArgumentException("CombinePaths one or both of the parameters cannot convert to string.");
            if (Path.IsPathFullyQualified(file)) throw new ArgumentException("CombinePaths allows only partial paths for the second argument");
            return new ParameterResolverValue(Path.Combine(path, file));
        }
        public ParameterResolverValue SaveFile(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Save file takes two arguments: file, save_path");
            IPostedFile file = args[0].Value as IPostedFile;
            string savepath = args[1].Value as string;
            if (savepath == null || file == null) throw new ArgumentException("SaveFile: either file, path or both are null");
            if (!Path.IsPathFullyQualified(savepath)) throw new ArgumentException("SaveFile the specified path is not fully qualified. Did you forget to combine paths?");
            using (var stream = new FileStream(savepath, FileMode.Create))
            {
                file.OpenReadStream().CopyTo(stream);
            }
            var scope = Scope(ctx);
            scope.DeleteOnRollback(savepath);
            return new ParameterResolverValue(true);
        }
        public ParameterResolverValue DeleteFile(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("DeleteFile takes single argument: filepath");
            string savepath = args[1].Value as string;
            if (savepath == null) throw new ArgumentException("DeleteFile: filepath is null or not a string");
            if (!Path.IsPathFullyQualified(savepath)) throw new ArgumentException("DeleteFile the specified path is not fully qualified. Did you forget to combine paths?");
            if (File.Exists(savepath))
            {
                var scope = Scope(ctx);
                scope.DeleteOnCommit(savepath);
            }
            return new ParameterResolverValue(true);
        }
        public ParameterResolverValue PrependFileName(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("PrependFileName requires two arguments (prefix, filename)");
            if (args[0].Value == null) throw new ArgumentException("PrependFileName prefix argument is null");
            var prefix = args[0].Value.ToString();
            var filename = args[1].Value as string;
            if (filename == null) throw new ArgumentException("PrependFileName filename is null or not a string");

            var name = $"{prefix}-{Path.GetFileNameWithoutExtension(filename)}{Path.GetExtension(filename)}";
            return new ParameterResolverValue(name);
        }
        public ParameterResolverValue CreateDirectory(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("CreateDirectory requires single parameter");
            var path = args[0].Value as string;
            if (path == null) throw new ArgumentNullException("CreateDirectory argument is null or not a string");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return new ParameterResolverValue(path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">(sring basedir, int spread, long id, IPostedFile file)</param>
        /// <returns></returns>
        public ParameterResolverValue SaveFileToSpread(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 4) throw new ArgumentException("SaveFileToSpread requires 4 arguments (sring basedir, int spread, long id, string filename)");
            string basedir;
            int spread;
            long id;
            IPostedFile file;
            try
            {
                basedir = args[0].Value as string;
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
            string filefullpath = Path.Combine(basedir, filespreaddir);
            // Saving
            using (var stream = new FileStream(filefullpath, FileMode.Create))
            {
                file.OpenReadStream().CopyTo(stream);
            }
            var scope = Scope(ctx);
            scope.DeleteOnRollback(filefullpath);
            return new ParameterResolverValue(filespreaddir);
        }
        

        #endregion

    }
}
