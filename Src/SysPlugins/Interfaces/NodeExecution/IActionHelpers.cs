using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces.NodeExecution {
    public interface IActionHelpers {
        ActionBase PerformedAction();
        bool ForCurrentAction<A>(Action<A> action) where A : ActionBase;
        A GetAction<A>() where A : ActionBase;
        bool OverAction<A>(Action<A> action) where A : ActionBase;

        IExecutionMeta NodeMeta { get; }
        Dictionary<string, ParameterResolverValue> NodeCache { get; }
    }
}
