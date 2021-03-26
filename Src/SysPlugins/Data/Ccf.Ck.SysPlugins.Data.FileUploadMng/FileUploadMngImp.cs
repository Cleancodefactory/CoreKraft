using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.FileUploadMng
{
    /// <summary>
    /// Query syntax
    /// 
    ///   Return(filename)
    ///   ReturnPreview(filename)
    ///   Store(filename, file)
    /// </summary>
    public class FileUploadMngImp : DataLoaderClassicBase<FileUploadMngSynchronizeContextScopedImp>
    {
        public FileUploadMngImp()
        {
        }

        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            Select s = Action<Select>(execContext);
            if (s != null)
            {
                //s.Query
            }
            return null;
        }

        protected override object Write(IDataLoaderWriteContext execContext)
        {
            throw new NotImplementedException();
        }
    }
}
