using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataLoaderReadContext: IDataLoaderContext {
        List<Dictionary<string, object>> Results { get; }
    }
}
