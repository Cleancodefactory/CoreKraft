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
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls
{
    [Library("internalcalls", LibraryContextFlags.MainNode)]
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
                case nameof(ScheduleCallRead):
                    return ScheduleCallRead;
                case nameof(ScheduleCallWrite):
                    return ScheduleCallWrite;
                case nameof(ScheduledCallStatus):
                    return ScheduledCallStatus;
                case nameof(ScheduledCallResult):
                    return ScheduledCallResult;
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

       

        private ParameterResolverValue _Call(bool isWrite,HostInterface ctx, ParameterResolverValue[] args,bool isIndirect = false)
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
            if (isIndirect) {

                if (ctx is ISupportsPluginServiceManager services) {

                    var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                    if (idc != null) {
                        Guid guid = idc.Call(inp, 0);
                        return new ParameterResolverValue(guid.ToString());
                    }
                } 
                throw new Exception("The context of the Function does not have access to the IndirectCalls service");
            } else {
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
        }
        /// <summary>
        /// Prototype(AC):
        /// CallRead(string, InternalDictionary) : ParamaterResolverValue|Dict|List
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(CallRead), "Executes read action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue CallRead(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args);
        }

        [Function(nameof(CallWrite), "Executes write action on the node in the nodeset specified by the address and returns the result. Make sure you set the state of the data (or parts of it) to insert, update or delete.")]
        [Parameter(0,"address","Address in the form module/nodeset[/node.path]",TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns",TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue CallWrite(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(true, ctx, args);
        }
        [Function(nameof(ScheduleCallRead), "Executes read action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue ScheduleCallRead(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args,true);
        }
        [Function(nameof(ScheduleCallWrite), "Executes read action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue ScheduleCallWrite(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(true, ctx, args, true);
        }

        public ParameterResolverValue ScheduledCallStatus(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is ISupportsPluginServiceManager services) {
                if (args.Length != 1) {
                    throw new ArgumentException("ScheduledCallStatus requires one argument - the id of the task");
                }
                string sid = Convert.ToString(args[0]);

                if (Guid.TryParse(sid, out Guid guid)) {
                    var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                    if (idc != null) {
                        IndirectCallStatus status = idc.CallStatus(guid);
                        return new ParameterResolverValue(status.ToString());
                    }
                }
            }
            return new ParameterResolverValue(IndirectCallStatus.Unavailable.ToString());
        }

        public ParameterResolverValue ScheduledCallResult(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is ISupportsPluginServiceManager services) {
                if (args.Length != 1) {
                    throw new ArgumentException("ScheduledCallStatus requires one argument - the id of the task");
                }
                string sid = Convert.ToString(args[0]);

                if (Guid.TryParse(sid, out Guid guid)) {
                    var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                    if (idc != null) {
                        dcall.ReturnModel ret = idc.GetResult(guid);
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
                }
            } else {
                throw new Exception("The context of the Function does not have access to the IndirectCalls service");
            }
            return new ParameterResolverValue(null);
        }

        #endregion

    }
}
