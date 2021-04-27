using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.Collections;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryBase<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {

        #region IActionQueryLibrary
        public virtual HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(Add):
                    return Add;
                case nameof(TryAdd):
                    return TryAdd;
                case nameof(Concat):
                    return Concat;
                case nameof(Cast):
                    return Cast;
                case nameof(GSetting):
                    return GSetting;
                case nameof(Throw):
                    return Throw;
                case nameof(IsEmpty):
                    return IsEmpty;
                case nameof(TypeOf):
                    return TypeOf;
                case nameof(IsNumeric):
                    return IsNumeric;
                case nameof(Random):
                    return Random;
                case nameof(Neg):
                    return Neg;
                case nameof(Equal):
                    return Equal;
                case nameof(Greater):
                    return Greater;
                case nameof(Lower):
                    return Lower;
                case nameof(Or):
                    return Or;
                case nameof(And):
                    return And;
                case nameof(Slice):
                    return Slice;
                case nameof(Length):
                    return Length;
                default:
                    return null;
            }
        }
        public virtual SymbolSet GetSymbols()
        {
            return new SymbolSet("Default library (no symbols)", null);
        }
        public void ClearDisposables()
        {
            // Nothing by default
        }
        #endregion

        #region Basic procedures

        #region Logical

        public ParameterResolverValue Or(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.Any(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }
        public ParameterResolverValue And(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length == 0) return new ParameterResolverValue(false);
            return new ParameterResolverValue(args.All(a => ActionQueryHostBase.IsTruthyOrFalsy(a)));
        }
        #endregion

        #region Arithmetic

        public ParameterResolverValue Random(HostInterface ctx, ParameterResolverValue[] args)
        {
            int min = 0;
            var random = new Random();
            if (args.Length > 0)
            {
                if (args[0].Value is int n)
                {
                    min = n;
                } 
                else if (args[0].Value is long l)
                {
                    min = (int)l;
                }
                if (args.Length > 1) { 
                    if (args[1].Value is int maxi)
                    {
                        return new ParameterResolverValue(random.Next(min, maxi));
                    } 
                    else if (args[1].Value is long maxl)
                    {
                        return new ParameterResolverValue(random.Next(min, (int)maxl));
                    } 
                    else
                    {
                        return new ParameterResolverValue(random.Next(min)); // min is max
                    }
                } 
                else
                {
                    return new ParameterResolverValue(random.Next(min)); // min is max
                }
            } 
            return new ParameterResolverValue(random.Next());
        }
        
        public ParameterResolverValue Add(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Any(a => a.Value is double || a.Value is float)) // Double result
            { 
                return new ParameterResolverValue(args.Sum(a => Convert.ToDouble(a.Value)));
            } 
            else if (args.Any(a => a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is byte || a.Value is char))
            {
                return new ParameterResolverValue(args.Sum(a => Convert.ToInt32(a.Value)));
            }
            else
            {
                return new ParameterResolverValue(null);
            }
        }
        public ParameterResolverValue Neg(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Neg needs single numeric argument");
            var v = args[0].Value;
            if (v is double || v is float)
            {
                return new ParameterResolverValue(-Convert.ToDouble(v));
            }
            else if (v is int || v is uint || v is short || v is ushort || v is char || v is byte)
            {
                return new ParameterResolverValue(-Convert.ToInt32(v));
            }
            else if (v is long || v is ulong)
            {
                return new ParameterResolverValue(-Convert.ToInt64(v));
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue TryAdd(HostInterface ctx, ParameterResolverValue[] args)
        {
            try
            {
                return Add(ctx, args);
            }
            catch 
            {
                return new ParameterResolverValue(null);
            }
        }

        #endregion

        #region Comparisons
        public ParameterResolverValue Equal(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Equal needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) == Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) == Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) == 0);
            }
        }
        public ParameterResolverValue Greater(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) > Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) > Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) > 0);
            }
        }
        public ParameterResolverValue Lower(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Lower needs two arguments");
            if (args.Length != 2) throw new ArgumentException("Greater needs two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ParameterResolverValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ParameterResolverValue(Convert.ToDouble(v1) < Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ParameterResolverValue(Convert.ToInt64(v1) < Convert.ToInt64(v2));
            }
            else
            {
                return new ParameterResolverValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) < 0);
            }
        }

        #endregion

        #region Strings

        public ParameterResolverValue Concat(HostInterface ctx, ParameterResolverValue[] args)
        {
            return new ParameterResolverValue(String.Concat(args.Select(a => a.Value != null ? a.Value.ToString() : "")));
        }
        public ParameterResolverValue IsEmpty(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsEmpty requires single argument.");
            var val = args[0].Value as string;
            return new ParameterResolverValue(string.IsNullOrWhiteSpace(val));
        }
        public ParameterResolverValue Slice(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length >= 2)
            {
                var str = Convert.ToString(args[0].Value);
                int start = Convert.ToInt32(args[1].Value);
                var end = str.Length;
                if (args.Length > 2)
                {
                    end = Convert.ToInt32(args[2].Value);
                }
                if (start >= 0 && start <= str.Length && end > start && end <= str.Length)
                {
                    return new ParameterResolverValue(str.Substring(start, end - start));
                } 
                else
                {
                    return new ParameterResolverValue(string.Empty);
                }
            } 
            else
            {
                throw new ArgumentException("Slice - incorrect number of arguments, 2 o3 are expected.");
            }
        }
        public ParameterResolverValue Length(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Length accepts exactly one argument.");
            if (args[0].Value is string s)
            {
                return new ParameterResolverValue(s.Length);
            }
            else if (args[0].Value is ICollection coll)
            {
                return new ParameterResolverValue(coll.Count);
            }
            return new ParameterResolverValue(null);
        }
        #endregion

        #region Typing

        public ParameterResolverValue Cast(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Cast requires two arguments.");
            string stype = args[0].Value as string;
            if (stype == null) throw new ArgumentException("Parameter 1 of Case must be string specifying the type to convert to (string,int,double,bool)");
            switch (stype)
            {
                case "string":
                    return new ParameterResolverValue(Convert.ToString(args[1].Value));
                case "bool":
                    return new ParameterResolverValue(Convert.ToBoolean(args[1].Value));
                case "int":
                    return new ParameterResolverValue(Convert.ToInt32(args[1].Value));
                case "long":
                    return new ParameterResolverValue(Convert.ToInt64(args[1].Value));
                case "double":
                    return new ParameterResolverValue(Convert.ToDouble(args[1].Value));
                default:
                    throw new ArgumentException("Parameter 1 contains unrecognized type name valida types are: string,int,long, double,bool");
            }
        }

        public ParameterResolverValue TypeOf(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("TypeOf requires one argument.");
            if (args[0].Value == null) return new ParameterResolverValue("null");
            if (args[0].Value is string) return new ParameterResolverValue("string");
            Type tc = args[0].Value.GetType();
            if (tc == typeof(int) || tc == typeof(uint)) return new ParameterResolverValue("int");
            if (tc == typeof(long) || tc == typeof(ulong)) return new ParameterResolverValue("long");
            if (tc == typeof(double) || tc == typeof(float)) return new ParameterResolverValue("double");
            if (tc == typeof(short) || tc == typeof(ushort)) return new ParameterResolverValue("short");
            if (tc == typeof(char) || tc == typeof(byte)) return new ParameterResolverValue("byte");
            if (tc == typeof(bool)) return new ParameterResolverValue("bool");

            return new ParameterResolverValue("unknown");
        }

        public ParameterResolverValue IsNumeric(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("IsNumeric requires one argument.");
            Type tc = args[0].Value.GetType();
            if (tc == typeof(int) || tc == typeof(uint) || tc == typeof(long) || tc == typeof(ulong)
                || tc == typeof(double) || tc == typeof(float) || tc == typeof(short) || tc == typeof(ushort) ||
                tc == typeof(char) || tc == typeof(byte)) return new ParameterResolverValue(true);

            return new ParameterResolverValue(false);
        }

        #endregion

        #endregion

        #region Settings
        public ParameterResolverValue GSetting(HostInterface _ctx, ParameterResolverValue[] args)
        {
            KraftGlobalConfigurationSettings kgcf = null;
            var ctx = _ctx as IDataLoaderContext;
            if (ctx != null)
            {
                kgcf = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            }
            else
            {
                var nctx = _ctx as INodePluginContext;
                if (nctx != null)
                {
                    kgcf = nctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                }
            }
            if (kgcf == null)
            {
                throw new Exception("Cannot obtain Kraft global settings");
            }
            if (args.Length != 1)
            {
                throw new ArgumentException($"GSetting accepts one argument, but {args.Length} were given.");
            }
            var name = args[0].Value as string;

            if (name == null)
            {
                throw new ArgumentException($"GSetting argument must be string - the name of the global kraft setting to obtain.");
            }
            switch (name)
            {
                case "EnvironmentName":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.EnvironmentName);
                case "ContentRootPath":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.ContentRootPath);
                case "ApplicationName":
                    return new ParameterResolverValue(kgcf.EnvironmentSettings.ApplicationName);
                case "StartModule":
                    return new ParameterResolverValue(kgcf.GeneralSettings.DefaultStartModule);
                case "ClientId":
                    return new ParameterResolverValue(kgcf.GeneralSettings.ClientId);

            }
            throw new ArgumentException($"The setting {name} is not supported");
        }

        #endregion

        public ParameterResolverValue Throw(HostInterface ctx, ParameterResolverValue[] args)
        {
            string extext = null;
            if (args.Length > 0)
            {
                if (args[0].Value is string)
                {
                    extext = args[0].Value as string;
                }
            } 
            else
            {
                extext = "Exception raised intentionally from an ActionQuery code";
            }
            throw new Exception(extext);
        }
    }
}
