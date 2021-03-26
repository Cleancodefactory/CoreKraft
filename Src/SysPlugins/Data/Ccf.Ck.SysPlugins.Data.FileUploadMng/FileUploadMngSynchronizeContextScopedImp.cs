using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Data.FileUploadMng
{
    public class FileUploadMngSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
