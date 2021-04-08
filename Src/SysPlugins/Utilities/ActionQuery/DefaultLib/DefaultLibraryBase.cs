using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryBase<HostInteface> : IActionQueryLibrary<HostInteface> where HostInteface : class
    {


        public virtual HostedProc<HostInteface> GetProc(string name)
        {
            switch (name)
            {
                case "Add":
                    return Add;
                case "TryAdd":
                    return TryAdd;
                case "Concat":
                    return Concat;
                case "Cast":
                    return Cast;
                default:
                    return null;
            }
        }

        #region Basic procedures
        public ParameterResolverValue Add(HostInteface ctx, ParameterResolverValue[] args)
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
        public ParameterResolverValue TryAdd(HostInteface ctx, ParameterResolverValue[] args)
        {
            try
            {
                return Add(ctx, args);
            }
            catch (Exception ex)
            {
                return new ParameterResolverValue(null);
            }
        }
        public ParameterResolverValue Concat(HostInteface ctx, ParameterResolverValue[] args)
        {
            return new ParameterResolverValue(String.Concat(args.Select(a => a.Value != null ? a.Value.ToString() : "")));
        }
        public ParameterResolverValue Cast(HostInteface ctx, ParameterResolverValue[] args)
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
                case "double":
                    return new ParameterResolverValue(Convert.ToDouble(args[1].Value));
                default:
                    throw new ArgumentException("Parameter 1 contains unrecognized type name valida types are: string,int,double,bool");
            }
        }

        #endregion
    }
}
