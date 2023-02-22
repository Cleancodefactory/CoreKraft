using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    /// <summary>
    /// Base class for all meta infos - ADOInfo, ActionQueryInfo etc.
    /// Reporters of meta data access the flags through this object.
    /// </summary>
    public class MetaInfoBase {
        public MetaInfoBase()
        {

        }

        public int LogicalExcutions { get; protected set; } = 1;

        public EMetaInfoFlags Flags { get; internal set; }
        public virtual void AddExecution() {  }
        public string ErrorInfo { get; protected set; }
        public virtual void SetErrorInfo(Exception ex, string additionalInfo) {
            if (!Flags.HasFlag(EMetaInfoFlags.Log)) return;
            StringBuilder stringBuilder= new StringBuilder();

            while (ex != null) {
                if (stringBuilder.Length > 0) { stringBuilder.Append("'|\n"); }
                stringBuilder.Append($"{ex.Message}");
                ex = ex.InnerException;
            }
            if (!string.IsNullOrWhiteSpace(additionalInfo)) stringBuilder.AppendLine(additionalInfo);
            this.ErrorInfo= stringBuilder.ToString();
        }
    }
}
