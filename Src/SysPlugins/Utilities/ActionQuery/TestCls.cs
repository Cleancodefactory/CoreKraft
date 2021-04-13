using System;
using System.Collections.Generic;
using System.Text;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Libs.ActionQuery;

namespace Ccf.Ck.SysPlugins.Utilities
{
    class TestCls
    {
        static void X()
        {
            IDataLoaderReadContext ctx = null;
            var h = new ActionQueryHost<IDataLoaderReadContext>(ctx)
            {
                {"A", Method1 },
                {"B", Method2 }
            };
            var ac = new ActionQuery<ParameterResolverValue>();
            var runner = ac.Compile("fsdf");
            runner.ExecuteScalar(h);
            
        }
        public static ParameterResolverValue Method1(IDataLoaderReadContext ctx, ParameterResolverValue[] args)
        {
            return default(ParameterResolverValue);
        }
        public static ParameterResolverValue Method2(IDataLoaderReadContext ctx, ParameterResolverValue[] args)
        {
            return default(ParameterResolverValue);
        }
    }
}
