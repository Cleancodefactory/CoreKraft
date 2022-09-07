using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataLoaderReadContext: IDataLoaderContext, INodePluginContextWithResults {
        List<Dictionary<string, object>> Results { get; }
    }
}
