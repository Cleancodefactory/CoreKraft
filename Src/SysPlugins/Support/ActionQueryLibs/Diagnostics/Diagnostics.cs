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
using Ccf.Ck.Models.Enumerations;


namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls
{
    [Library("diagnostics", LibraryContextFlags.MainNode)]
    public class DiagnosticsLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        
        private object _LockObject = new Object();
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(CallSchedulerInfo):
                    return CallSchedulerInfo;
                case nameof(CallSchedulerThreads):
                    return CallSchedulerThreads;
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
        private string FormatCallAddress(dcall.InputModel im) {
            return string.Format("{0}/{1}/{2}", im.Module, im.Nodeset, im.Nodepath);
        }
        #endregion

        #region Functions
        public ParameterResolverValue CallSchedulerThreads(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is ISupportsPluginServiceManager services) {
                var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                if (idc is IIndirectCallerControl callcontrol) {
                    var info = callcontrol.GetIndirectServiceThreadInfo();
                    if (info.Threads != null) {
                        var data = new List<ParameterResolverValue>();
                        var count = info.Threads.Count();
                        foreach (var ti in info.Threads) {
                            var dict = new Dictionary<string, ParameterResolverValue>();
                            dict["DirectCallAvailable"] = new ParameterResolverValue(ti.DirectCallAvailable);
                            dict["LastTaskCompleted"] = new ParameterResolverValue(ti.LastTaskCompleted);
                            dict["LastTaskPickedAt"] = new ParameterResolverValue(ti.LastTaskPickedAt);
                            dict["TaskPicked"] = new ParameterResolverValue(ti.TaskPicked);
                            dict["Executing"] = new ParameterResolverValue(ti.Executing);
                            dict["FinishHandler"] = new ParameterResolverValue(ti.FinishtHandler);
                            dict["LastTaskCompleted"] = new ParameterResolverValue(ti.LastTaskCompleted);
                            dict["LastTaskFinishedAt"] = new ParameterResolverValue(ti.LastTaskFinishedAt);
                            dict["Looping"] = new ParameterResolverValue(ti.Looping);
                            dict["LoopingEnded"] = new ParameterResolverValue(ti.LoopingEnded);
                            dict["StartHandler"] = new ParameterResolverValue(ti.StartHandler);
                            dict["ThreadIndex"] = new ParameterResolverValue(ti.ThreadIndex);
                            data.Add(new ParameterResolverValue(dict));
                        }
                        return new ParameterResolverValue(data);
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue CallSchedulerInfo(HostInterface ctx, ParameterResolverValue[] args) {
            bool bIncludeData = false;
            if (args.Length > 0) {
                bIncludeData = Convert.ToBoolean(args[0].Value);
            }
            if (ctx is ISupportsPluginServiceManager services) {
                var idc = services.PluginServiceManager.GetService<IIndirectCallService>(typeof(IIndirectCallService));
                if (idc is IIndirectCallerControl callcontrol) {
                    var info = callcontrol.GetIndirectServiceInfo();
                    if (info != null) {
                        Dictionary<string, ParameterResolverValue> dinfo = new Dictionary<string, ParameterResolverValue>();
                        dinfo["waiting"] = new ParameterResolverValue(null);
                        if (info.Waiting != null) {
                            dinfo["waiting"] = new ParameterResolverValue( info.Waiting.Select(ti => {
                                var tinfo = new Dictionary<string, ParameterResolverValue>();
                                tinfo["scheduleid"] = new ParameterResolverValue(ti.ScheduleId.ToString());
                                tinfo["call"] = new ParameterResolverValue(FormatCallAddress(ti.Input));
                                if (bIncludeData) {
                                    tinfo["input"] = new ParameterResolverValue(ti.Input);
                                    tinfo["result"] = new ParameterResolverValue(ti.Result);
                                }
                                tinfo["scheduled"] = new ParameterResolverValue(ti.Scheduled);
                                tinfo["finished"] = new ParameterResolverValue(ti.Finished);
                                tinfo["started"] = new ParameterResolverValue(ti.Started);
                                tinfo["status"] = new ParameterResolverValue(ti.Status.ToString());
                                return new ParameterResolverValue(tinfo);
                            }).ToList());
                        }
                        dinfo["finished"] = new ParameterResolverValue(null);
                        if (info.Finished != null) {
                            dinfo["finished"] = new ParameterResolverValue(info.Finished.Select(ti => {
                                var tinfo = new Dictionary<string, ParameterResolverValue>();
                                tinfo["scheduleid"] = new ParameterResolverValue(ti.ScheduleId.ToString());
                                tinfo["call"] = new ParameterResolverValue(FormatCallAddress(ti.Input));
                                if (bIncludeData) {
                                    tinfo["input"] = new ParameterResolverValue(ti.Input);
                                    tinfo["result"] = new ParameterResolverValue(ti.Result);
                                }
                                tinfo["scheduled"] = new ParameterResolverValue(ti.Scheduled);
                                tinfo["finished"] = new ParameterResolverValue(ti.Finished);
                                tinfo["started"] = new ParameterResolverValue(ti.Started);
                                tinfo["status"] = new ParameterResolverValue(ti.Status.ToString());
                                return new ParameterResolverValue(tinfo);
                            }).ToList());
                        }
                        return new ParameterResolverValue(dinfo);
                    } else {
                        return new ParameterResolverValue(null);
                    }
                }
                throw new Exception("The CallSchedulerInfo cannot obtain control interface to IndirectCalls service");
            } else {
                throw new Exception("The context of the CallSchedulerInfo does not have access to the IndirectCalls service");
            }
        }
        

        #endregion

    }
}
