using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ck.SysPlugins.Iterators.DataNodes
{
    public class CompiledParameter
    {
        public CompiledParameter(string name) {
            Name = name;
        }
        public string Name { get; set; }
        /// <summary>
        /// Compiled resolverrunner
        /// </summary>
        public ResolverRunner<ParameterResolverValue, IParameterResolverContext> Resolver {get; set;}
       // public ResolverRunner<ParameterResolverValue, IParameterResolverContext> Validator { get; set; }
    }
}
