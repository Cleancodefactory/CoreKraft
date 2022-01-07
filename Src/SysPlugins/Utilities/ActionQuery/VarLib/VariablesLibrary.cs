using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class VariablesLibrary<HostInteface> : IActionQueryLibrary<HostInteface> where HostInteface : class
    {
        private Dictionary<string, ParameterResolverValue> _Variables = new Dictionary<string, ParameterResolverValue>();

        public ParameterResolverValue SetVar(string name, ParameterResolverValue value) {
            if (name != null) {
                _Variables[name] = value;
                return value;
            } else {
                throw new ArgumentException("The expected name of variable is null or not a string");
            }
        }
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
        public ParameterResolverValue Undefine(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("Undefine requires at least one argument. All the arguments are names of variables to undefine.");
            }
            var count = args.Length;
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                var name = args[i].Value as string;
                
                if (name != null)
                {
                    _Variables.Remove(name);
                    n++;
                }
                else
                {
                    throw new ArgumentException($"Undefine - expected name of variable is not a string at argument index {i}.");
                }
            }
            return new ParameterResolverValue(n);
        }
        public ParameterResolverValue GetVar(string name) {
            if (name != null) {
                if (_Variables.ContainsKey(name)) {
                    return _Variables[name];
                } else {
                    return new ParameterResolverValue(null);
                }
            } else {
                throw new ArgumentException("The name of the variable is null or not a string");
            }
        }
        public ParameterResolverValue Get(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Get requires single argument - the name of the variable to get.");
            }
            var name = args[0].Value as string;
            return GetVar(name);
        }
        public ParameterResolverValue Inc(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Inc requires single argument - the name of the variable to increment.");
            }
            var name = args[0].Value as string;
            if (name != null)
            {
                if (_Variables.ContainsKey(name))
                {
                    var v = _Variables[name];
                    int n = Convert.ToInt32(v.Value);
                    v.Value = n + 1;
                    _Variables[name] = v;
                    return v;
                }
                else
                {
                    return new ParameterResolverValue(null);
                }
            }
            else
            {
                throw new ArgumentException("Inc requires string argument - the name of the variable, but got something else.");
            }
        }
        public ParameterResolverValue Dec(HostInteface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Dec requires single argument - the name of the variable to increment.");
            }
            var name = args[0].Value as string;
            if (name != null)
            {
                if (_Variables.ContainsKey(name))
                {
                    var v = _Variables[name];
                    int n = Convert.ToInt32(v.Value);
                    v.Value = n - 1;
                    _Variables[name] = v;
                    return v;
                }
                else
                {
                    return new ParameterResolverValue(null);
                }
            }
            else
            {
                throw new ArgumentException("Dec requires string argument - the name of the variable, but got something else.");
            }
        }

        #region IActionQueryLibrary
        public HostedProc<HostInteface> GetProc(string name)
        {
            switch (name)
            {
                case "Get":
                    return Get;
                case "Set":
                    return Set;
                case "Inc":
                    return Inc;
                case "Dec":
                    return Dec;
                case "Undefine":
                    return Undefine;
            }
            return null;
        }
        public virtual SymbolSet GetSymbols()
        {
            return new SymbolSet("Variables library", _Variables.Select(kv => new SymbolEntry(kv.Key, kv.Value)));
        }
        public void ClearDisposables()
        {
            _Variables = null;
        }
        #endregion
    }
}
