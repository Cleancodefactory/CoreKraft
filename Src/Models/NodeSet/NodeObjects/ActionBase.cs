using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.Models.NodeSet
{
    public abstract class ActionBase
    {
        public ActionBase()
        {
        }

        public string File
        {
            get;
            set;
        }

        public string LoadQuery
        {
            get;
            set;
        }

        public string Query
        {
            get;
            set;
        }

        public Dictionary<string, object> Json
        {
            get;
            set;
        }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public bool IsTypeOf(string ext)
        {
            if (this.File != null)
            {
                FileInfo fi = new(this.File);
                if (string.IsNullOrEmpty(fi.Extension))
                {
                    throw new Exception($"File: {fi.FullName} does not have an extension. Please recheck!");
                }
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