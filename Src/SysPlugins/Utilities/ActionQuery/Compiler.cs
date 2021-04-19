using Ccf.Ck.Libs.ActionQuery;
using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class Compiler
    {
        private readonly static ActionQuery<ParameterResolverValue> _instance = new ActionQuery<ParameterResolverValue>();
        public static ActionQueryRunner<ParameterResolverValue> Compile(string query)
        {
            return _instance.Compile(query);
        }
    }
}
