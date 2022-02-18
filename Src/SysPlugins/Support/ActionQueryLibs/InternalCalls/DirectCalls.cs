using Ccf.Ck.Models.DirectCall;
using dcall = Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls
{
    [Library("inteernalcalls", LibraryContextFlags.MainNode)]
    public class DirectCallLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        
        private object _LockObject = new Object();
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(CallRead):
                   return CallRead;
                case nameof(CallWrite):
                    return CallWrite;
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Internal calls library (no symbols)", null);
        }

        private List<object> _disposables = new List<object>();
        public void ClearDisposables()
        {
            lock (_LockObject)
            {
                for (int i = 0; i < _disposables.Count; i++)
                {
                    if (_disposables[i] is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                }
                _disposables.Clear();
            }
        }
        #endregion

        #region Helpers
        private static Regex reAddress = new Regex(@"^([a-zA-Z][a-zA-Z0-9\-\._]*)/([a-zA-Z][a-zA-Z0-9\-\._]*)(?:/([a-zA-Z][a-zA-Z0-9\-\._]*))?$", RegexOptions.Compiled);
        /// <summary>
        /// Parses the address into module, nodeset, nodepath
        /// 
        /// </summary>
        /// <param name="address">Syntax is: module/nodeset/nodepath</param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool ParseCallAddress(string address, dcall.InputModel input)
        {
            Match m = reAddress.Match(address);
            if (m.Success)
            {
                if (m.Groups[0].Success)
                {
                    input.Module = m.Groups[1].Value;
                    if (m.Groups[2].Success)
                    {
                        input.Nodeset = m.Groups[2].Value;
                        if (m.Groups[3].Success)
                        {
                            input.Nodepath = m.Groups[3].Value;
                        } 
                        else
                        {
                            input.Nodepath = null;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Functions

        private ParameterResolverValue _Call(bool isWrite,HostInterface ctx, ParameterResolverValue[] args)
        {
            dcall.InputModel inp = new dcall.InputModel() { IsWriteOperation = isWrite };
            ReturnModel ret = null;

            if (args.Length < 1) throw new ArgumentException("CallRead needs at least one argument - the address to call.");
            if (!ParseCallAddress(Convert.ToString(args[0].Value), inp)) {
                return Error.Create("Address syntax error.");
            }
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> indict) {
                    if (isWrite)
                    {
                        var data = DefaultLibraryBase<HostInterface>.ConvertToGenericData(indict);
                        if (data is Dictionary<string,object> _dictData)
                        {
                            inp.Data = _dictData;
                        } else
                        {
                            // TODO: This should not happen
                            KraftLogger.LogError("in _Call The passed Data dictionary did not convert correctly", indict);
                        }
                    } else
                    {
                        inp.Data = indict.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
                    }
                    
                } else {
                    throw new ArgumentException("Main arguments are currently supported only as a internal AC Dictionary (Dictionary<string, ParameterResolverValue>). Use Dict and related functions from the default library to create one.");
                }
                if (args.Length > 2) {
                    if (args[2].Value is Dictionary<string, ParameterResolverValue> qdict) {
                        inp.QueryCollection = qdict.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
                    } else {
                        throw new ArgumentException("Query collection arguments are currently supported only as a Dictionary. Use Dict and related functions from the default library to create one.");
                    }
                }
            }
            ret = DirectCallService.Instance.Call(inp);
            if (ret.IsSuccessful) {
                if (ret.BinaryData is IPostedFile pf) {
                    return new ParameterResolverValue(pf);
                } else if (ret.Data != null) {
                    return ret.Data switch {
                        Dictionary<string, object> => DefaultLibraryBase<HostInterface>.ConvertFromGenericData(ret.Data),
                        List<Dictionary<string, object>> => DefaultLibraryBase<HostInterface>.ConvertFromGenericData(ret.Data),
                        _ => new ParameterResolverValue(null)
                    };
                } else {
                    return new ParameterResolverValue(null);
                }
            } else {
                return Error.Create(ret.ErrorMessage);
            }
        }
        /// <summary>
        /// Prototype(AC):
        /// CallRead(string, InternalDictionary) : ParamaterResolverValue|Dict|List
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(CallRead), "")]
        public ParameterResolverValue CallRead(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args);
        }

        [Function(nameof(CallWrite), "")]
        public ParameterResolverValue CallWrite(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(true, ctx, args);
        }
        #endregion

    }
}
