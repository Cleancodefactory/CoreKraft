﻿using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BuiltIn;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BuitIn;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.Interfaces;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.Managers
{
    /// <summary>
    /// Holds to all the ParameterResolverSets loaded in memory and provides resolver delegates when requested.
    /// The delegates and the underlying methods are assumed to be immutable and once loaded they are cached.
    /// Any violation of tis immutability requirement will cause troubles.
    /// </summary>
    public class ParameterResolversManager : IParameterResolversSource, IParameterResolverSetManager, IResolverFinder<ParameterResolverValue, IParameterResolverContext>
    {

        #region Singleton - this will probably move to a DI

        public static ParameterResolversManager Instance
        {
            get
            {
                return SingletonHolder.instance;
            }
        }
        protected class SingletonHolder
        {
            static SingletonHolder()
            {
                // TODO: See how are we going to proceed with the configuration
                // BUILT-IN only
                instance.AddSet(new BuiltInParameterResolverSet1_0(
                    new ResolverSet()
                    {
                        GlobalNames = true,
                        Name = "BuiltIn",
                        Resolvers = new List<Resolver>()
                        {
                            new Resolver()
                                {
                                     Alias = "GetFrom",
                                     Arguments = 2,
                                     Name = "GetFrom"
                                },
                            new Resolver() {
                                    Alias = "CombineSources",
                                    Arguments = 1,
                                    Name = "CombineSources"
                                },
                            new Resolver() {
                                    Alias = "TryGetFrom",
                                    Arguments= 2,
                                    Name = "TryGetFrom"
                                },
                            {
                                new Resolver() {
                                    Alias = "NavGetFrom",
                                    Arguments= 2,
                                    Name = "NavGetFrom"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "CurrentData",
                                    Arguments = 0,
                                    Name = "CurrentData"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetHostingUrl",
                                     Arguments = 0,
                                     Name = "GetHostingUrl"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GlobalSetting",
                                     Arguments = 1,
                                     Name = "GlobalSetting"
                                }
                            },
                            { 
                                new Resolver() {
                                    Alias = "NavGetGlobalSetting",
                                    Arguments = 1,
                                    Name = "NavGetGlobalSetting"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "NavGetSlaveSetting",
                                    Arguments = 2,
                                    Name = "NavGetSlaveSetting"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "ModuleName",
                                     Arguments = 0,
                                     Name = "ModuleName"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "UrlBase",
                                     Arguments = 1,
                                     Name = "UrlBase"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "RequestType",
                                     Arguments = 0,
                                     Name = "RequestType"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "RequestProcessor",
                                     Arguments = 0,
                                     Name = "RequestProcessor"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "RequestTask",
                                     Arguments = 0,
                                     Name = "RequestTask"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "NewGuid",
                                     Arguments = 0,
                                     Name = "NewGuid"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetUserId",
                                     Arguments = 0,
                                     Name = "GetUserId"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetAuthBearerToken",
                                     Arguments = 0,
                                     Name = "GetAuthBearerToken"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetAuthorizationPasswordEndPoint",
                                     Arguments = 0,
                                     Name = "GetAuthorizationPasswordEndPoint"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetUserEmail",
                                     Arguments = 0,
                                     Name = "GetUserEmail"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "HasRoleName",
                                     Arguments = 1,
                                     Name = "HasRoleName"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Or",
                                     Arguments = 2,
                                     Name = "Or"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "And",
                                     Arguments = 2,
                                     Name = "And"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Not",
                                     Arguments = 1,
                                     Name = "Not"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Concat",
                                     Arguments = 2,
                                     Name = "Concat"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Replace",
                                     Arguments = 3,
                                     Name = "Replace"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Coalesce",
                                     Arguments = 2,
                                     Name = "Coalesce"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "NumAsText",
                                     Arguments = 2,
                                     Name = "NumAsText"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "IsEmpty",
                                    Arguments = 1,
                                    Name = "IsEmpty"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "NullIfEmpty",
                                    Arguments = 1,
                                    Name = "NullIfEmpty"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "CastAs",
                                     Arguments = 2,
                                     Name = "CastAs"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "To8601String",
                                     Arguments = 1,
                                     Name = "To8601String"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Add",
                                     Arguments = 2,
                                     Name = "Add"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Sub",
                                     Arguments = 2,
                                     Name = "Sub"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "Random",
                                    Arguments = 2,
                                    Name = "Random"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "Once",
                                    Arguments = 2,
                                    Name = "Once"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "IfThenElse",
                                    Arguments = 3,
                                    Name = "IfThenElse"
                                }

                            },
                            {
                                new Resolver() {
                                    Alias = "Equal",
                                    Arguments = 2,
                                    Name = "Equal"
                                }

                            },
                            {
                                new Resolver() {
                                    Alias = "Greater",
                                    Arguments = 2,
                                    Name = "Greater"
                                }

                            },
                            {
                                new Resolver() {
                                    Alias = "Lower",
                                    Arguments = 2,
                                    Name = "Lower"
                                }

                            },
                            {
                                new Resolver()
                                {
                                    Alias = "AsContent",
                                    Arguments = 1,
                                    Name = "AsContent"
                                }
                            },
                            {
                                  new Resolver() {
                                      Alias = "CheckedText",
                                      Arguments = 2,
                                      Name = "CheckedText"
                                  }
                            },
                            {
                                new Resolver()
                                {
                                    Alias = "OrderByEntry",
                                    Arguments = 3,
                                    Name = "OrderByEntry"
                                }
                            },
                            {
                                new Resolver()
                                {
                                    Alias = "OrderBy",
                                    Arguments = 1,
                                    Name = "OrderBy"

                                }
                            },
                            {
                                new Resolver()
                                {
                                    Alias = "OrderBy2",
                                    Arguments = 2,
                                    Name = "OrderBy"

                                }
                            },
                            {
                                new Resolver()
                                {
                                    Alias = "OrderBy3",
                                    Arguments = 3,
                                    Name = "OrderBy"

                                }
                            },
                            //{
                            //    new Resolver()
                            //    {
                            //        Alias = "ApiTokenFromAuth",
                            //        Arguments = 1,
                            //        Name = "ApiTokenFromAuth"
                            //    }
                            //},
                            {
                                new Resolver()
                                {
                                     Alias = "GetUserDetails",
                                     Arguments = 1,
                                     Name = "GetUserDetails"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "GetUserRoles",
                                     Arguments = 0,
                                     Name = "GetUserRoles"
                                }
                            },
                            ////////////////////////// TEST ONES ///////////////////
                            {
                                new Resolver()
                                {
                                     Alias = "Skip",
                                     Arguments = 0,
                                     Name = "Skip"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "IntegerContent",
                                     Arguments = 1,
                                     Name = "IntegerContent"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "idlist",
                                     Arguments = 2,
                                     Name = "idlist"
                                }
                            },
                            {
                                new Resolver() // Alias to compensate for the mistake above
                                {
                                     Alias = "IdList",
                                     Arguments = 2,
                                     Name = "idlist"
                                }
                            },
                            {
                                new Resolver() // Padded version
                                {
                                     Alias = "IdList",
                                     Arguments = 3,
                                     Name = "idlistPadded"
                                }
                            },
                            {
                                new Resolver()
                                {
                                     Alias = "Split",
                                     Arguments = 2,
                                     Name = "Split"
                                }
                            },
                            //////////////////////////// META /////////////////////////
                            {
                                new Resolver() {
                                    Alias = "MetaNode",
                                    Arguments = 2,
                                    Name = "MetaNode"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "MetaADOResult",
                                    Arguments = 2,
                                    Name = "MetaADOResult"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "MetaRoot",
                                    Arguments = 2,
                                    Name = "MetaRoot"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "PostedFileContentType",
                                    Arguments = 1,
                                    Name = "PostedFileContentType"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "PostedFileLength",
                                    Arguments = 1,
                                    Name = "PostedFileLength"
                                }
                            },
                            {
                                new Resolver() {
                                    Alias = "PostedFileName",
                                    Arguments = 1,
                                    Name = "PostedFileName"
                                }
                            }
                        }
                    }
                ));
                instance.AddSet(new BoardResolverSet1_0(
                    new ResolverSet()
                    {
                        GlobalNames = true,
                        Name = "Board",
                        Resolvers = new List<Resolver>()
                        {
                            new Resolver()
                            {
                                    Alias = "GetPermission",
                                    Arguments = 1,
                                    Name = "GetPermission"
                            },
                            new Resolver()
                            {
                                    Alias = "GenerateHash",
                                    Arguments = 0,
                                    Name = "GenerateHash"
                            }
                        }
                    }));

                // KraftHRM resolvers
                instance.AddSet(new HrmResolverSet1_0(new ResolverSet()
                {

                    GlobalNames = true,
                    Name = "KraftHRM",
                    Resolvers = new List<Resolver>()
                        {
                            new Resolver()
                            {
                                    Alias = "GetHrmUserPermission",
                                    Arguments = 1,
                                    Name = "GetHrmUserPermission"

                            }
                    }
                }));

                instance.AddSet(new HrmResolverSet1_0(new ResolverSet()
                {

                    GlobalNames = true,
                    Name = "KraftHRM",
                    Resolvers = new List<Resolver>()
                        {
                            new Resolver()
                            {
                                    Alias = "GetHrmEdgePermission",
                                    Arguments = 1,
                                    Name = "GetHrmEdgePermission"

                            }
                    }
                }));

                instance.AddSet(new HrmResolverSet1_0(new ResolverSet()
                {

                    GlobalNames = true,
                    Name = "KraftHRM",
                    Resolvers = new List<Resolver>()
                        {
                            new Resolver()
                            {
                                    Alias = "GetHrmEmployeesInProject",
                                    Arguments = 1,
                                    Name = "GetHrmEmployeesInProject"

                            }
                    }
                }));

            }
            internal static readonly ParameterResolversManager instance = new ParameterResolversManager(false);
        }

        #endregion

        /// <summary>
        /// A compiler instance - the compiler is immutable so we keep it within this singleton and it also becomes one that way.
        /// </summary>
        public Expression.ParameterExpression Compiler { get; private set; } = new Expression.ParameterExpression();

        private object registerDelegateLocker = new object();

        public bool FullChainNaming { get; private set; }
        /// <summary>
        /// Contains the registered sets for run-time (first use) query.
        /// </summary>
        private List<ParameterResolverSet> _Sets = new List<ParameterResolverSet>();
        /// <summary>
        /// Contains the already created delegates for quick response to requesters.
        /// The delegates are cached under their full names.
        /// </summary>
        private Dictionary<string, ResolverDelegate<ParameterResolverValue, IParameterResolverContext>> _delegates = new Dictionary<string, ResolverDelegate<ParameterResolverValue, IParameterResolverContext>>();

        public ParameterResolversManager(bool fullNames = false)
        {
            FullChainNaming = fullNames;
        }

        /// <summary>
        /// Registers a resolver set with the manager. The prefix is intended for module names when registering resolvers read from module configuration.
        /// Prefixes can be used by some built-in or global packages which is a matter of resolver naming standardization
        /// In fullNames mode the resolvers will have long names - up to 3 parts - prefix.setname.resolver, when this mode is off the setname is not used in resolver naming.
        /// </summary>
        /// <param name="resolverSet">Loaded ParameterResolverSet</param>
        /// <param name="prefix">Optional prefix to add to the resolver names for all the resolvers in the set.</param>
        /// <param name="config">Usually the configuration should be already loaded in the ParameterResolverSet, but we allow it to be set late (but only once) in order to support some variations for the loading process. Can be removed later.</param>
        /// <returns></returns>
        public void AddSet(ParameterResolverSet resolverSet) // Old, string prefix = null, ResolverSet config = null)
        {
            // TODO: Perhaps we will allow configuration, but only controllint the behavior of the resolver set and not accessed from outside
            // The commented code is from before when the registration allowed for prefixes and names kept in the registration.
            if (resolverSet == null) throw new ArgumentNullException("resolverSet cannot be null");
            _Sets.Add(resolverSet);
            // Synch is not needed because the manager is created, filled and only after that set in the KraftModule.
            //if (config != null) resolverSet.Configuration = config; // TODO: We can change that in future to depend on the configuration passed on creation.
            //_Sets.Add(new SetRegistration(resolverSet, prefix, resolverSet.Name));
        }


        /// <summary>
        /// Returns resolver from the _delegates as REsolverDelegate, ready to use or if it is not present there it first searches for it and creates it 
        /// from one of the registered ParameterResolverSet-s.
        /// Changes in the concept may be confusung if old sorce is compared to the current. So, it is important to notice tha while the old version was never put into 
        /// production, it aimed at adding resolvers to the entire system globally from all the modules, hence the complicated naming conventions (module.resolverset.resolver).
        /// After reviewing the concept we found out that the usage of the resolvers is simpler and more to the point if viewed as usage of a plugin thus tunrning the old concept into
        /// something more similar to the other kind of plugins:
        /// - Resolvers are implemented in libraries called sets as methods of "set" classes, then all these configured in a specific module are loaded for the module only. If the same set
        /// is needed in other modules, it has to be configured there too. The implementation will not be duplicated, just used in every module where it is configured.
        /// - The naming scheme is simplified and can expose the resolvers as global names or as dotted notation (prefix.resolver) where the prefix is the name of the resolver set.
        /// 
        /// The new approach does not need global uniqueness and makes simpler and shorter naming scheme possible, also it treats the "sets" as kind of plugins which fits the general 
        /// concept everybody is familiar with (from the other kinds of plugins).
        /// </summary>
        /// <param name="name">The alias of the method or the full name under which it is egistered - see the class notes for details on naming.</param>
        /// <returns></returns>
        public ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver(string name) {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty or null, It is expected to have the form: [<prefix>.]<resolvername>");
            if (_delegates.ContainsKey(name)) return _delegates[name];
            string[] parts = name.Split('.');
            string prefix = null;
            string alias = null;
            if (parts.Length == 2) {
                prefix = parts[0].Trim();
                alias = parts[1].Trim();
            } else if (parts.Length == 1) {
                alias = parts[0].Trim();
                prefix = null;
            } else {
                // Wrong
                throw new ArgumentException("name cannot be empty or contain more than one dot. It is expected to have the form: [<prefix>.]<resolvername>");
            }
            ResolverDelegate<ParameterResolverValue, IParameterResolverContext> resolver = null;

            foreach (var _set in _Sets) {
                if (prefix == null) {
                    resolver = _set.GetResolver(alias);
                    if (resolver != null) return resolver;
                } else {
                    if (!String.IsNullOrWhiteSpace(_set.Name) && String.Compare(_set.Name, prefix,StringComparison.Ordinal) == 0) {
                        resolver = _set.GetResolver(alias);
                    }
                }
            }
            if (resolver != null) {
                lock (this.registerDelegateLocker) {
                    if (!_delegates.ContainsKey(name)) {
                        _delegates[name] = resolver;
                    }
                }
                return resolver;
            }
            return null;
        }
        #region Old code for information - no longer needed
        //public ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver_old(string name)
        //{
        //    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty or null, It is expected to have the form: [<prefix>.]<resolvername>");
        //    if (_delegates.ContainsKey(name)) return _delegates[name];
        //    // We have to find it
        //    string[] parts = name.Split('.');
        //    string prefix = null;
        //    string setname = null;
        //    string alias = null;
        //    if (parts.Length == 1)
        //    {
        //        alias = parts[0].Trim();
        //    }
        //    else if (parts.Length == 2)
        //    {
        //        prefix = parts[0].Trim();
        //        alias = parts[1].Trim();
        //    }
        //    else if (parts.Length == 3)
        //    {
        //        if (!FullChainNaming)
        //        {
        //            // TODO: Error reporting - this cannot be correct
        //            // If the manager had this set previously all resolved names under fullName policy will be in the cache.
        //            return null;
        //        }
        //        prefix = parts[0].Trim();
        //        setname = parts[1].Trim();
        //        alias = parts[2].Trim();
        //    }
        //    else
        //    {
        //        // TODO: Error reporting - this cannot be correct
        //        return null;
        //    }
        //    ResolverDelegate<ParameterResolverValue, IParameterResolverContext> resolver = null;
        //    if (parts.Length == 1)
        //    {
        //        // This can happen only for specially configured methods or sets - fullNames are ignored, prefixes are also ignored for them
        //        // So, any resolver will have at least 2 part name if it or its set (library) is not marked appropriately
        //        var libs = _Sets.Where(s => s.Library.Configuration != null)
        //                    .Select(s => s.Library)
        //                    .Where(l =>
        //                        (l.Configuration.GlobalNames && l.Configuration.Resolvers.Any(r => r.Alias == alias)) ||
        //                        (l.Configuration.Resolvers.Any(r => r.GlobalName && r.Alias == alias))
        //                    ).ToList();
        //        if (libs.Count == 0) return null; // Not found
        //        if (libs.Count > 1) throw new Exception("Ambiguous name exception - more than one library is configured to register resolver with that name.");
        //        resolver = libs[0].GetResolver(alias);
        //        lock (this.registerDelegateLocker)
        //        {
        //            if (!_delegates.ContainsKey(name))
        //            {
        //                _delegates[name] = resolver;
        //            }
        //        }
        //        return resolver;
        //    }
        //    else if (parts.Length == 2)
        //    {
        //        // LibName.Alias - We can have 2 parts only when the name of the set and the name of the resolver are both used
        //        // but fullChainNaming is off
        //        if (!FullChainNaming)
        //        {
        //            var libs = _Sets.Where(s => s.Name == setname).Select(s => s.Library).Where(l => l.Configuration != null && !l.Configuration.GlobalNames).ToList();
        //            if (libs.Count == 0) return null; // Not found - there is no library that can possibly give us this one
        //            libs = libs.Where(l => l.Configuration.Resolvers.Any(r => !r.GlobalName && r.Alias == alias)).ToList();
        //            if (libs.Count == 0) return null; // Not found
        //            if (libs.Count > 1) throw new Exception("Ambiguous name - more than one resolver are found for this name.");
        //            resolver = libs[0].GetResolver(alias);
        //            lock (this.registerDelegateLocker)
        //            {
        //                if (!_delegates.ContainsKey(name))
        //                {
        //                    _delegates[name] = resolver;
        //                }
        //            }
        //            return resolver;
        //        }
        //    }
        //    else if (parts.Length == 3)
        //    {
        //        // Full names - if we are here it is permitted, but lets check it for better readability
        //        // This is much like case 2, but with FullChanin mode on
        //        if (FullChainNaming)
        //        {
        //            var libs = _Sets.Where(s => s.Name == setname && s.Prefix == prefix).Select(s => s.Library).Where(l => l.Configuration != null && !l.Configuration.GlobalNames).ToList();
        //            if (libs.Count == 0) return null; // Not found - there is no library that can possibly give us this one
        //            libs = libs.Where(l => l.Configuration.Resolvers.Any(r => !r.GlobalName && r.Alias == alias)).ToList();
        //            if (libs.Count == 0) return null; // Not found
        //            if (libs.Count > 1) throw new Exception("Ambiguous name - more than one resolver are found for this name.");
        //            resolver = libs[0].GetResolver(alias);
        //            lock (this.registerDelegateLocker)
        //            {
        //                if (!_delegates.ContainsKey(name))
        //                {
        //                    _delegates[name] = resolver;
        //                }
        //            }
        //            return resolver;
        //        }
        //    }
        //    return null;

        //}

        // private List<>
        //protected struct SetRegistration
        //{
        //    public SetRegistration(ParameterResolverSet lib, string prefix, string name)
        //    {
        //        Library = lib;
        //        Prefix = prefix;
        //        Name = name;

        //    }
        //    public ParameterResolverSet Library;
        //    public string Prefix;
        //    public string Name; // We use the name from the Library, but we keep the name here in order to be able to change more easilly this policy if neccessary. 

        //}
        #endregion
    }
}
