using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.FileUpload
{
    public class FileUploadSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
