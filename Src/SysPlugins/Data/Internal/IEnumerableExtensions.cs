using System.Collections;

namespace Ccf.Ck.SysPlugins.Data.Internal
{
    internal static class IEnumerableExtensions
    {
        internal static bool Any(this IEnumerable source)
        {
            foreach (var item in source)
            {
                return true;
            }

            return false;
        }
    }
}
