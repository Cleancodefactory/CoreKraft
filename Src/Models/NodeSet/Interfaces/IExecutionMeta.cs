using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    /// <summary>
    /// Supported only by the MetaNode, should be extended with infos for the diferrent plugins with default implementations for their properties based on the methods here
    /// </summary>
    public interface IExecutionMeta {

        T GetInfo<T>() where T: MetaInfoBase, new();

        T CreateInfo<T>() where T : MetaInfoBase, new();
    }
}
