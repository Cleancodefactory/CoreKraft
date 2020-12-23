using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.Interfaces;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses
{
    /// <summary>
    /// The purpose of this class
    /// </summary>
    public abstract class ParameterResolverSet : IParameterResolversSource
    {
        private ParameterResolverSet() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="name">Override the name from the configuration</param>
        protected ParameterResolverSet(ResolverSet conf, string name = null)
        {
            Name = name;
            Configuration = conf;
        }
        protected ParameterResolverSet(string name)
        {
            Name = name;
        }
        public string Name { get; protected set; }
        private ResolverSet _configuration;
        /// <summary>
        /// This property can be set only once!
        /// No errors are issued, but this feature enables different loading procedures compared to keeping this possible only in the constructor.
        /// </summary>
        public ResolverSet Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                if (_configuration == null)
                {
                    _configuration = value;
                    if (_configuration != null && Name == null)
                    {
                        Name = _configuration.Name;
                    }
                }
            }
        }

        private ConcurrentDictionary<string, ResolverDelegate<ParameterResolverValue, IParameterResolverContext>> _Cache = new ConcurrentDictionary<string, ResolverDelegate<ParameterResolverValue, IParameterResolverContext>>();

        private bool CacheMethod(string mname, int numargs = 0, string alias = null)
        {
            TypeInfo type = this.GetType().GetTypeInfo();
            try
            {
                MethodInfo mi = type.GetMethod(mname, new Type[] { typeof(IParameterResolverContext), typeof(IList<ParameterResolverValue>) });
                if (mi != null)
                {
                    if (mi.ReturnType == typeof(ParameterResolverValue))
                    {   // Ok this is a method matching our wishes
                        try
                        {
                            Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue> rawdelegate =
                                mi.CreateDelegate(typeof(Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue>), this) as Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue>;

                            _Cache.TryAdd((alias ?? mname), new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(rawdelegate, numargs, String.Format("{0}[{1}]", mname, numargs)));
                            return _Cache.ContainsKey(alias ?? mname);
                        }
                        catch
                        {

                            return false;
                            // TODO: Consider this option: throw new Exception("Failed to create delegate from the specified method");
                        }
                    }
                }
                else if (numargs == 0)
                {
                    ///// GENERATION FROM MORE HUMAN FRIENDLY READABLE METHOD DECLARATIONS /////
                    /* Examples:
                     * public ParameterResolverValue MyResolver1(IParameterResolverContext ctx, ParameterResolverValue arg1, ParameterResolverValue arg2)
                     * public ParameterResolverValue MyResolver2(IParameterResolverContext ctx)
                     * public ParameterResolverValue MyResolver3()
                     * Not currently supported kind:
                     * public ParameterResolverValue MyResolver4(ParameterResolverValue arg1, ParameterResolverValue arg1)
                     * i.e. methods having arguments, but not taking the context as first argument are not currently allowed 
                     * the reason is promotion of possibly wrong decisions - complex resolvers are too likely to need the context.
                     * 
                     * In this case numargs is ignored - it is determined by the method declaration instead.
                     * HOWEVER, to make sure that the requester knows that, we require the numargs to be 0!
                     * 
                     */
                    mi = type.GetMethod(mname);
                    if (mi != null && mi.ReturnType == typeof(ParameterResolverValue))
                    {
                        ParameterInfo[] args = mi.GetParameters();
                        if (args == null || args.Length == 0)
                        {
                            ///// SIMPLE METHOD with absolutely no arguments /////
                            Func<ParameterResolverValue> _raw = mi.CreateDelegate(typeof(Func<ParameterResolverValue>), this) as Func<ParameterResolverValue>;
                            Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue> rawdelegate = (v, l) =>
                            {
                                return _raw();
                            };
                            _Cache.TryAdd((alias ?? mname), new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(rawdelegate, 0, String.Format("{0}[0]", mname)));
                            return _Cache.ContainsKey(alias ?? mname);
                        }
                        if (args != null && args.Length >= 1 && args[0].ParameterType == typeof(IParameterResolverContext))
                        {
                            if (args.Length == 1)
                            {
                                ///// SIMPLE METHOD with IParameterResolverContext parameter only /////
                                Func<IParameterResolverContext, ParameterResolverValue> _raw = mi.CreateDelegate(typeof(Func<IParameterResolverContext, ParameterResolverValue>), this) as Func<IParameterResolverContext, ParameterResolverValue>;
                                Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue> rawdelegate = (v, l) =>
                                {
                                    return _raw(v);
                                };
                                _Cache.TryAdd((alias ?? mname), new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(rawdelegate, 0, String.Format("{0}[0]", mname)));
                                return _Cache.ContainsKey(alias ?? mname);
                            }
                            else
                            {
                                ///// SIMPLE METHOD with IParameterResolverContext and a few ParameterResolverValue arguments specified as individual parameters /////
                                int nargs = args.Length - 1;
                                List<Type> argtypes = new List<Type>() { typeof(IParameterResolverContext) };
                                for (int i = 1; i < args.Length; i++)
                                {
                                    if (args[i].ParameterType != typeof(ParameterResolverValue)) return false;
                                    argtypes.Add(typeof(ParameterResolverValue));
                                }
                                // And add the return result - always there
                                argtypes.Add(typeof(ParameterResolverValue));
                                Type t = typeof(Func<>);
                                Type retype = t.MakeGenericType(argtypes.ToArray());

                                var _raw = mi.CreateDelegate(retype);
                                Func<IParameterResolverContext, IList<ParameterResolverValue>, ParameterResolverValue> rawdelegate = (v, l) =>
                                {
                                    object[] margs = new object[nargs + 1];
                                    margs[0] = v;
                                    if (nargs != l.Count) throw new ArgumentException("Argument count does not match definition in " + alias + "/" + mname);
                                    for (int i = 0; i < l.Count; i++)
                                    {
                                        if (i > nargs - 1) throw new ArgumentException("Too many arguments passed to " + alias + "/" + mname);
                                        margs[i + 1] = l[i];
                                    }
                                    return (ParameterResolverValue)_raw.DynamicInvoke(margs);
                                };
                                _Cache.TryAdd((alias ?? mname), new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(rawdelegate, nargs, String.Format("{0}[{1}]", mname, nargs)));
                                return _Cache.ContainsKey(alias ?? mname);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Nothing for now
            }
            return false;
        }

        public ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver(string alias)
        {
            var r =  GetResolverEx(alias);
            if (r == null) throw new Exception($"Cannot find the resolver with alias: {alias}");
            return r;
        }
        public ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolverEx(string alias, string name = null, int numargs = 0)
        {
            if (!_Cache.ContainsKey(alias))
            {
                // this method can be called with alias only and will use the Configuration then.
                //  When called with a name and numargs it will ignore the confifuration and try create mapping directly.
                //  The second method should be used mostly for internal tests and not in production.
                if (!string.IsNullOrWhiteSpace(name))
                {
                    // directly
                    if (!CacheMethod(name, numargs, alias))
                    {
                        return null;
                    }
                }
                else
                {
                    // from configuration
                    if (Configuration != null)
                    {
                        Resolver r = Configuration.Resolvers.FirstOrDefault(x => x.Alias == alias);
                        if (r != null)
                        {
                            if (!CacheMethod(r.Name, r.Arguments, r.Alias))
                            {
                                return null;
                            }
                        }
                    }

                }
            }
            if (_Cache.ContainsKey(alias))
            {
                return _Cache[alias];
            }
            return null;
        }
    }
}
