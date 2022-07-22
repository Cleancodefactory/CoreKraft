using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    /// <summary>
    /// Modelling level interface for access to the different kinds of meta information carried by the MetaNode. MetaNode 
    /// supports this interface and wherever piece of code has access to a MetaNode it can query the specific model attached in it
    /// through a specific property.
    /// </summary>
    public interface IBuiltInExecutionMeta: IExecutionMeta {
        ADOInfo ADOInfo { 
            get { 
                return GetInfo<ADOInfo>();
            } 
        }
        ActionQueryInfo ActionQuery {
            get { return GetInfo<ActionQueryInfo>(); }
        }
    }
}
