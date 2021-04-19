using System;
using System.IO;

namespace Ccf.Ck.Models.NodeSet
{
    public partial class ActionBase
    {
        public bool IsTypeOf(string ext)
        {
            if (this.File != null)
            {
                FileInfo fi = new FileInfo(this.File);
                string extension = fi.Extension.Substring(1);
                return ext.Equals(extension, StringComparison.CurrentCultureIgnoreCase);
            }

            return false;
        }
        public bool HasLoadQuery() => !string.IsNullOrEmpty(this.LoadQuery);

        public bool HasFile() => !string.IsNullOrEmpty(this.File);        

        // public bool HasJSON() => (Json != null && Json.Count > 0);

        public bool HasStatement() => !string.IsNullOrEmpty(Query);
    }
}