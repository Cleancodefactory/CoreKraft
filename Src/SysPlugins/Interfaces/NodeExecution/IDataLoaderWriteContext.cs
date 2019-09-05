using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataLoaderWriteContext: IDataLoaderContext {
        Dictionary<string, object> Row { get; }
    }
}
