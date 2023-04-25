using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Utilities;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls
{
    [Library("internalcalls", LibraryContextFlags.MainNode)]
    public class DirectCallLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        
        private readonly object _LockObject = new Object();
        private readonly CallSchedulerCallHandlers _Handlers = new CallSchedulerCallHandlers();
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(CallRead):
                    return CallRead;
                case nameof(CallNew):
                    return CallNew;
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
                case nameof(OnSchedule):
                    return OnSchedule;
                case nameof(OnStart):
                    return OnStart;
                case nameof(OnFinish):
                    return OnFinish;
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Internal calls library (no symbols)", null);
        }

        private readonly List<object> _Disposables = new List<object>();
        public void ClearDisposables()
        {
            lock (_LockObject)
            {
                for (int i = 0; i < _Disposables.Count; i++)
                {
                    if (_Disposables[i] is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                }
                _Disposables.Clear();
            }
        }
        #endregion

        #region Helpers
        private void SetSchedulerHandling(Ccf.Ck.Models.DirectCall.InputModel input) {
            input.SchedulerCallHandlers = _Handlers.CloneOrNull();
        }

        private static readonly Regex _ReAddress = new Regex(@"^([a-zA-Z][a-zA-Z0-9\-\._]*)/([a-zA-Z][a-zA-Z0-9\-\._]*)(?:/([a-zA-Z][a-zA-Z0-9\-\._]*))?$", RegexOptions.Compiled);
        /// <summary>
        /// Parses the address into module, nodeset, nodepath
        /// 
        /// </summary>
        /// <param name="address">Syntax is: module/nodeset/nodepath</param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool ParseCallAddress(string address, Ccf.Ck.Models.DirectCall.InputModel input)
        {
            Match m = _ReAddress.Match(address);
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

       

        private ParameterResolverValue _Call(bool isWrite,HostInterface ctx, ParameterResolverValue[] args,bool isIndirect = false, EReadAction readAction = EReadAction.Default)
        {

            Ccf.Ck.Models.DirectCall.InputModel inp;
            if (args.Length > 3 && !string.IsNullOrEmpty(args[3].Value as string)) {
                if (ctx is IDataLoaderContext lctx) {
                    inp = lctx.PrepareCallModelAs(runas: args[3].Value as string ,isWriteOperation: isWrite, readAction: readAction);
                } else if (ctx is INodePluginContext cctx) {
                    inp = cctx.PrepareCallModelAs(runas: args[3].Value as string, isWriteOperation: isWrite, readAction: readAction);
                } else {
                    inp = new Ccf.Ck.Models.DirectCall.InputModel() { IsWriteOperation = isWrite, ReadAction = readAction };
                }
            } else {
                if (ctx is IDataLoaderContext lctx) {
                    inp = lctx.PrepareCallModel(isWriteOperation: isWrite, readAction: readAction);
                } else if (ctx is INodePluginContext cctx) {
                    inp = cctx.PrepareCallModel(isWriteOperation: isWrite, readAction: readAction);
                } else {
                    inp = new Ccf.Ck.Models.DirectCall.InputModel() { IsWriteOperation = isWrite, ReadAction = readAction };
                }
            }
            ReturnModel ret = null;

            
                

            if (args.Length < 1) throw new ArgumentException("CallRead needs at least one argument - the address to call.");
            if (!ParseCallAddress(Convert.ToString(args[0].Value), inp)) {
                return Error.Create("Address syntax error.");
            }
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> indict)
                {
                    if (isWrite)
                    {
                        var data = DefaultLibraryBase<HostInterface>.ConvertToGenericData(indict);
                        if (data is Dictionary<string, object> _dictData)
                        {
                            inp.Data = _dictData;
                        }
                        else
                        {
                            // TODO: This should not happen
                            KraftLogger.LogError("in _Call The passed Data dictionary did not convert correctly", indict);
                        }
                    }
                    else
                    {
                        inp.Data = DefaultLibraryBase<HostInterface>.ConvertToGenericData(indict) as Dictionary<string, object>;
                    }
                }
                else if (args[1].Value is Dictionary<string, object> rawdict)
                {
                    inp.Data = rawdict;
                }
                else if (args[1].Value == null)
                {
                    inp.Data = new Dictionary<String,object>();
                
                } else {
                    throw new ArgumentException("Main arguments are currently supported only as a internal AC Dictionary (Dictionary<string, ParameterResolverValue>). Use Dict and related functions from the default library to create one.");
                }
                if (args.Length > 2) {
                    if (args[2].Value is Dictionary<string, ParameterResolverValue> qdict) {
                        inp.QueryCollection = qdict.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
                    } else if (args[2].Value is Dictionary<string, object> rawdict) {
                        inp.QueryCollection = rawdict;
                    } else if (args[2].Value == null)  {
                        // leave it empty  or null or whatever
                    } else {
                        throw new ArgumentException("Query collection arguments are currently supported only as a Dictionary. Use Dict and related functions from the default library to create one.");
                    }
                }
            }
            if (isIndirect) {

                if (ctx is ISupportsPluginServiceManager services) {

                    var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                    if (idc != null) {
                        SetSchedulerHandling(inp);
                        Guid guid = idc.Call(inp, 0);
                        if (guid == Guid.Empty) {
                            return Error.Create("Cannot schedule the call.");
                        } else {
                            return new ParameterResolverValue(guid.ToString());
                        }
                        
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
        [Parameter(3, "runas", "A string with builtin username (see authorization section in global settings)", TypeFlags.String | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue CallRead(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args);
        }
        [Function(nameof(CallNew), "Executes new action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Parameter(3, "runas", "A string with builtin username (see authorization section in global settings)", TypeFlags.String | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue CallNew(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args,false,EReadAction.New);
        }

        [Function(nameof(CallWrite), "Executes write action on the node in the nodeset specified by the address and returns the result. Make sure you set the state of the data (or parts of it) to insert, update or delete.")]
        [Parameter(0,"address","Address in the form module/nodeset[/node.path]",TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Parameter(3, "runas", "A string with builtin username (see authorization section in global settings)", TypeFlags.String | TypeFlags.Optional)]
        [Result("Result from the node converted to script usable List or Dict depending on what the node returns", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue CallWrite(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(true, ctx, args);
        }
        [Function(nameof(ScheduleCallRead), "Schedules read action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON. The data is preserved until the task is scheduled.", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters. The data is preserved until the task is scheduled.", TypeFlags.Dict | TypeFlags.Optional)]
        [Parameter(3, "runas", "A string with builtin username (see authorization section in global settings)", TypeFlags.String | TypeFlags.Optional)]
        [Result("TaskId guid string or error", TypeFlags.String | TypeFlags.Error)]
        public ParameterResolverValue ScheduleCallRead(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(false, ctx, args,true);
        }
        [Function(nameof(ScheduleCallWrite), "Schedules write action on the node in the nodeset specified by the address and returns the result")]
        [Parameter(0, "address", "Address in the form module/nodeset[/node.path]", TypeFlags.String)]
        [Parameter(1, "data", "A dictionary accessible like posted JSON", TypeFlags.Dict)]
        [Parameter(2, "clientdata", "A dictionary of query string parameters", TypeFlags.Dict | TypeFlags.Optional)]
        [Parameter(3, "runas", "A string with builtin username (see authorization section in global settings)", TypeFlags.String | TypeFlags.Optional)]
        [Result("TaskId guid string or error", TypeFlags.String | TypeFlags.Error)]
        public ParameterResolverValue ScheduleCallWrite(HostInterface ctx, ParameterResolverValue[] args) {
            return _Call(true, ctx, args, true);
        }

        [Function(nameof(ScheduledCallStatus), "Attempts to obtain the scheduiling status of a scheduled task")]
        [Parameter(0, "taskid", "A guid as string", TypeFlags.String)]
        [Result("One of: Queued, Running, Finished, Discarded or Unavailable", TypeFlags.String)]
        public ParameterResolverValue ScheduledCallStatus(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is ISupportsPluginServiceManager services) {
                if (args.Length != 1) {
                    throw new ArgumentException("ScheduledCallStatus requires one argument - the id of the task");
                }
                string sid = Convert.ToString(args[0].Value);

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

        [Function(nameof(ScheduledCallResult),"Attempts to obtain the result of a finished scheduled task")]
        [Parameter(0, "taskid", "A guid as string", TypeFlags.String)]
        [Result("Result converted to script usable List or Dict depending on what the task result is (the same as CallRead), null is returned if the result is not available. It is recommended to check status first.", TypeFlags.Dict | TypeFlags.List | TypeFlags.PostedFile | TypeFlags.Error | TypeFlags.Null)]
        public ParameterResolverValue ScheduledCallResult(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is ISupportsPluginServiceManager services) {
                if (args.Length != 1) {
                    throw new ArgumentException("ScheduledCallStatus requires one argument - the id of the task");
                }
                string sid = Convert.ToString(args[0].Value);

                if (Guid.TryParse(sid, out Guid guid)) {
                    var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                    if (idc != null) {
                        Ccf.Ck.Models.DirectCall.ReturnModel ret = idc.GetResult(guid);
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

        private ParameterResolverValue OnCallback(Action<CallSchedulerCallHandlers,CallSchedulerHandler> _set, ParameterResolverValue[] args) {
            CallSchedulerHandler handler;
            if (args.Length > 0) {
                string address = Convert.ToString(args[0].Value);
                if (string.IsNullOrEmpty(address)) {
                    _set(_Handlers, null);
                    return new ParameterResolverValue(null);
                }
                handler = new CallSchedulerHandler() { Address = address, RunAs = null, IsWriteOperation = false }; // For clarity
                if (args.Length > 1) {
                    if (Convert.ToBoolean(args[1].Value)) {
                        handler.IsWriteOperation = true;
                    }
                }
                if (args.Length > 2) {
                    var s = Convert.ToString(args[2].Value);
                    if (string.IsNullOrWhiteSpace(s)) {
                        handler.RunAs = null;
                    } else {
                        handler.RunAs = s;
                    }
                    
                }
                _set(_Handlers, handler);
                return new ParameterResolverValue(address);
            }
            throw new ArgumentException("The address argument is required. Pass null if you want to remove handler");
        }

        [Function(nameof(OnSchedule),"Sets the callback node for scheduled calls")]
        [Parameter(0,"address", "The address to call when the task is scheduled. If null the callback is removed.", TypeFlags.String|TypeFlags.Null)]
        [Parameter(1, "write", "true id write call should be make", TypeFlags.Bool)]
        [Parameter(2, "runas", "Built in username if needed", TypeFlags.String | TypeFlags.Optional)]
        [Result("The address", TypeFlags.String | TypeFlags.Null)]
        public ParameterResolverValue OnSchedule(HostInterface ctx, ParameterResolverValue[] args) {
            return OnCallback((hs,h) => hs.OnCallScheduled = h,args);
        }

        [Function(nameof(OnStart), "Sets the callback node for scheduled calls")]
        [Parameter(0, "address", "The address to call when the task is scheduled. If null the callback is removed.", TypeFlags.String | TypeFlags.Null)]
        [Parameter(1, "write", "true id write call should be make", TypeFlags.Bool)]
        [Parameter(2, "runas", "Built in username if needed", TypeFlags.String | TypeFlags.Optional)]
        [Result("The address", TypeFlags.String | TypeFlags.Null)]
        public ParameterResolverValue OnStart(HostInterface ctx, ParameterResolverValue[] args) {
            return OnCallback((hs, h) => hs.OnCallStarted = h, args);
        }
        [Function(nameof(OnFinish), "Sets the callback node for scheduled calls")]
        [Parameter(0, "address", "The address to call when the task is scheduled. If null the callback is removed.", TypeFlags.String | TypeFlags.Null)]
        [Parameter(1, "write", "true id write call should be make", TypeFlags.Bool)]
        [Parameter(2, "runas", "Built in username if needed", TypeFlags.String | TypeFlags.Optional)]
        [Result("The address", TypeFlags.String | TypeFlags.Null)]
        public ParameterResolverValue OnFinish(HostInterface ctx, ParameterResolverValue[] args) {
            return OnCallback((hs, h) => hs.OnCallFinished = h, args);
        }

        #endregion

    }
}
