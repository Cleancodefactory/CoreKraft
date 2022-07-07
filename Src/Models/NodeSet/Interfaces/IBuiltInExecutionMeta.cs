using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public interface IBuiltInExecutionMeta: IExecutionMeta {
        ADOInfo ADOInfo { 
            get { 
                return GetInfo<ADOInfo>();
            } 
            set {
                SetInfo<ADOInfo>(value);
            }
        }
        ActionQueryInfo ActionQuery {
            get { return GetInfo<ActionQueryInfo>(); }
            set { SetInfo(value); }
        }
    }
}
