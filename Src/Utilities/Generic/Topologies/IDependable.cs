using System.Collections.Generic;

namespace Ccf.Ck.Utilities.Generic.Topologies
{
    public interface IDependable<T>
    {
        int DependencyOrderIndex { get; }
        string Key { get; }
        IDictionary<string, IDependable<T>> Dependencies { get; }
    }
}
