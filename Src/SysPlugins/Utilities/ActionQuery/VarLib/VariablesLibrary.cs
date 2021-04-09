using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class VariablesLibrary<HostInteface> : IActionQueryLibrary<HostInteface> where HostInteface : class
    {
        private Dictionary<string, ParameterResolverValue> _Variables = new Dictionary<string, ParameterResolverValue>();


        public ParameterResolverValue Set(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Set requires even number of arguments (varname, value, varname, value , ....");
            }
            var count = args.Length / 2;
            var lastvalue = new ParameterResolverValue();
            for (int i = 0; i < count; i++)
            {
                var name = args[i * 2].Value as string;
                var value = args[i * 2 + 1];
                if (name != null)
                {
                    _Variables[name] = value;
                    lastvalue = value;
                }
                else
                {
                    throw new ArgumentException($"Set - expected name of variable is not a string at argument index {i * 2}.");
                }
            }
            return lastvalue;
        }
        public ParameterResolverValue Get(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Get requires single argument - the name of the variable to get.");
            }
            var name = args[0].Value as string;
            if (name != null)
            {
                if (_Variables.ContainsKey(name))
                {
                    return _Variables[name];
                }
                else
                {
                    return new ParameterResolverValue(null);
                }
            } 
            else
            {
                throw new ArgumentException("Get requires string argument - the name of the variable, but got something else.");
            }
        }
        public HostedProc<HostInteface> GetProc(string name)
        {
            switch (name)
            {
                case "Get":
                    return Get;
                case "Set":
                    return Set;
            }
            return null;
        }
    }
}
