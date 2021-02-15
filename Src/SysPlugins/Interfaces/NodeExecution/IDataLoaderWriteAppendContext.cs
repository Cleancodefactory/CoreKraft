using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataLoaderWriteAppendContext: IDataLoaderContext {
        ReadOnlyDictionary<string, object> Row { get; }
        List<Dictionary<string, object>> AppendResults { get; }
    }
}