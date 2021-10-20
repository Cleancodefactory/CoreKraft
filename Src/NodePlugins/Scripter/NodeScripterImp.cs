using Ccf.Ck.NodePlugins.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;

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
                        if (execContext.OwnContextScoped.CustomSettings.TryGetValue("libraries", out string libs) && !string.IsNullOrWhiteSpace(libs)) {
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
                                        //st.AddLibrary(new WebLibrary<Context>());
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



        public ParameterResolverValue HostInfo(IDataLoaderContext ctx, ParameterResolverValue[] args) {
            return new ParameterResolverValue("Scripter 1.0");
        }

        private string GetQuery(INodePluginContext context) {

        }
    }
}
