using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class ADOInfo: MetaInfoBase {
        public ADOInfo() {
        }

        public int RowsAffected { get; set; }

        private List<Result> _Results;
        public ReadOnlyCollection<Result> Results { get {
                return new ReadOnlyCollection<Result>(_Results);
            } 
        }

        #region plugin reporting
        public void AddResult(int rows, int fields) {
            if (Flags.HasFlag(EMetaInfoFlags.Basic)) {
                if (_Results == null) _Results = new List<Result>();
                _Results.Add(new Result(rows, fields));
            }
        }


        #endregion

        public record Result (int Rows, int Fields);
    }
        
}
