using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class ADOInfo: MetaInfoBase {

        #region Nested declarations
        public record Result(int Rows, int Fields);
        public class Execution
        {
            public int RowsAffected { get; set; }
            private Stack<Result> _Results = new Stack<Result>();
            #region Methods
            public IEnumerable<Result> Results { 
                get {
                    return _Results;
                }
            }
            public void AddResult(Result r)
            {
                _Results.Push(r);
            }
            #endregion

        }
        #endregion
        public ADOInfo():base() {
            _Executions.Push(new Execution());
        }

        public override void AddExecution()
        {
            LogicalExcutions++;
            _Executions.Push(new Execution());
        }
        private Stack<Execution> _Executions = new Stack<Execution>();

        protected Execution _TopExecution { get
            {
                return _Executions.Peek();
            } 
        }

        public IEnumerable<Execution> ExecutionsLog
        {
            get
            {
                return _Executions.ToArray();
            }
        }

        public IEnumerable<IEnumerable<Result>> ResultsLog { 
            get {
                return ExecutionsLog.Select(e => e.Results);
            } 
        }

        #region plugin reporting
        public int RowsAffected
        {
            get
            {
                return _TopExecution.RowsAffected;
            }
            set
            {
                _TopExecution.RowsAffected = value;
            }
        }
        public void AddResult(int rows, int fields) {
            if (Flags.HasFlag(EMetaInfoFlags.Basic)) {
                _TopExecution.AddResult(new Result(rows, fields));
            }
        }


        #endregion

        
    }
        
}
