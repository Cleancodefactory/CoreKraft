using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.NodePlugins.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Files;
using Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb;
using Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Images;
using Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using System.Linq;

namespace Ccf.Ck.NodePlugins.Scripter {
    public class NodeScripterImp: NodePluginBase<NodeScripterSynchronizeContextScopedImp> {

        private const string PLUGIN_INTERNAL_NAME = "NodeScripterImp";

        #region NodePluginBase
        protected override void ExecuteRead(INodePluginReadContext execContext) {
            ExecuteQuery(execContext);
        }

        protected override void ExecuteWrite(INodePluginWriteContext execContext) {
            ExecuteQuery(execContext);
        }

        protected virtual void ExecuteQuery<Context>(Context execContext) where Context : class, INodePluginContext {
            bool trace = execContext.CurrentNode.Trace;

            string qry = GetQuery(execContext);
            if (qry != null) {
                try {
                    var runner = Compiler.Compile(qry);
                    if (runner.ErrorText != null) {
                        KraftLogger.LogError($"{execContext.LocationInfo(PLUGIN_INTERNAL_NAME)}\n{runner.ErrorText}");
                        throw new Exception(runner.ErrorText);
                    }
                    using (var host = new ActionQueryHost<Context>(execContext) {
                            { "HostInfo", HostInfo }
                        }) {
                        if (execContext.OwnContextScoped.CustomSettings != null &&
                            execContext.OwnContextScoped.CustomSettings.TryGetValue("libraries", out string libs) &&
                            !string.IsNullOrWhiteSpace(libs)) {
                            var _ = libs.Split(',').Distinct();
                            foreach (var v in _) {
                                switch (v) {
                                    case "basicimage":
                                        host.AddLibrary(new BasicImageLib<Context>());
                                        break;
                                    case "basicweb":
                                        host.AddLibrary(new WebLibrary<Context>());
                                        break;
                                    case "files":
                                        host.AddLibrary(new BasicFiles<Context>());
                                        break;
                                    case "internalcalls":
                                        host.AddLibrary(new DirectCallLib<Context>());
                                        break;

                                }
                            }

                        }
                        if (trace) {
                            host.Trace = true;
                        }
                        try {
                            var result = runner.ExecuteScalar(host, ActionQueryHost<Context>.HardLimit(execContext));
                        } catch {
                            if (trace) {
                                var traceInfo = host.GetTraceInfo();
                                if (traceInfo != null) {
                                    KraftLogger.LogError($"{execContext.LocationInfo(PLUGIN_INTERNAL_NAME)}\n");
                                    KraftLogger.LogError(traceInfo.ToString());
                                }
                            }
                            throw;
                        }
                    }
                } catch (Exception ex) {

                    KraftLogger.LogError(ActionQueryTrace.ExceptionToString(ex));
                    throw;
                }
            }
        }
        #endregion



        public ParameterResolverValue HostInfo(INodePluginContext ctx, ParameterResolverValue[] args) {
            return new ParameterResolverValue("NodeScripter 1.1");
        }

        private string GetQuery(INodePluginContext context) {
            if (context.CustomPlugin != null) {
                return context.CustomPlugin.Query;
            }
            return null;
        }
    }
}
