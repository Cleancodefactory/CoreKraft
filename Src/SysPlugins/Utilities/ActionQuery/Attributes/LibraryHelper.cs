using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    public static class LibraryHelper
    {
        /*
        delegate ParameterResolverValue AQProc<I,HostInterface>(I instance, HostInterface ctx, ParameterResolverValue[] args);
        private static Dictionary<string, MethodInfo> _info = null;
        static void _CollectData<I, HostInterface>()
        {
            Type typeL = typeof(I);
            MethodInfo[] minfos = typeL.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var mi in minfos)
            {
                var d = (AQProc<I,HostInterface>)Delegate.CreateDelegate(typeL, null, mi);

                d.
                
                
                var dlg = mi.CreateDelegate<AQProc<HostInterface>>();
                var cloen = dlg.Clone();
                dlg.T
                dlg(null,null);
                KeyValuePair<string, MemberInfo> ksmi = new KeyValuePair<string, MemberInfo>(mi.Name, mi);
                
            }

        }
        //public static HostedProc<L> GetAutoProc<L>(this L inst, string name) where L : class, IActionQueryLibrary<HostInterface>
        //{

        //}
        */
    }
}
