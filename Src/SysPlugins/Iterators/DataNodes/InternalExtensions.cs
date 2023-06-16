using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ck.SysPlugins.Iterators.DataNodes;
using NUglify.JavaScript.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes {
    internal static class InternalExtensions {

        #region Read extensions
        // Not needed - using this is impossible or should be based on pre-plugins
        /// <summary>
        /// Orders and returns the portion of the children for pre or post execution
        /// </summary>
        /// <param name="children"></param>
        /// <param name="action"></param>
        /// <param name="preLoader"></param>
        /// <returns></returns>
        internal static List<Node> ForReadExecution(this List<Node> children, EReadAction action, bool preLoader = false) {
            if (children == null) return null;
            if (preLoader) {
                return children.Where(n => n.ReadExecutionOrder(action) < 0).OrderBy(n => n.ReadExecutionOrder(action)).ToList();
            } else {
                return children.Where(n => n.ReadExecutionOrder(action) >= 0).OrderBy(n => n.ReadExecutionOrder(action)).ToList();
            }
        }
        internal static List<Node> OrderForReadExecution(this List<Node> children, EReadAction action) {
            if (children == null) return null;
            return children.OrderBy(n => n.ReadExecutionOrder(action)).ToList();
        }
        internal static int ReadExecutionOrder(this Node node, EReadAction action) {
            int ord = node.ExecutionOrder;
            if (node.Read != null) {
                if (node.Read.ExecutionOrder != 0) {
                    ord = node.Read.ExecutionOrder;
                }
                if (action == EReadAction.Select) {
                    if (node.Read.Select != null) {
                        if (node.Read.Select.ExecutionOrder != 0) {
                            ord = node.Read.Select.ExecutionOrder;
                        }
                    }

                } else if (action == EReadAction.New && node.Read.New != null) {
                    if (node.Read.New.ExecutionOrder != 0) {
                        ord = node.Read.New.ExecutionOrder;
                    }
                }
            }
            return ord;
        }
        #endregion

        #region Write extensions
        internal static List<Node> ForWriteExecution(this List<Node> children, string operation, bool preLoaded = false) {
            if (children == null) return null;
            if (preLoaded) {
                return children.Where(n => n.WriteExecutionOrder(operation) < 0).OrderBy(n => n.WriteExecutionOrder(operation)).ToList();
            } else {
                return children.Where(n => n.WriteExecutionOrder(operation) >= 0).OrderBy(n => n.WriteExecutionOrder(operation)).ToList();
            }
        }
        internal static int WriteExecutionOrder(this Node node, string operation) {
            int ord = node.ExecutionOrder;
            if (node.Write != null) {
                if (node.Write.ExecutionOrder != 0) {
                    ord = node.Write.ExecutionOrder;
                }
                ord = operation switch {
                    OPERATION_INSERT => node.Write?.Insert?.ExecutionOrder ?? ord,
                    OPERATION_UPDATE => node.Write?.Update?.ExecutionOrder ?? ord,
                    OPERATION_DELETE => node.Write?.Delete?.ExecutionOrder ?? ord,
                    _ => ord
                };
            }
            return ord;
        }
        #endregion

        #region Resolver Cachiing extensions

        public static CompiledParameter GetReadCachedParameter(this Node node, string name)
        {
            if (node == null) return null;
            return node.ReadCache as CompiledParameter;
        }
        public static CompiledParameter GetWriteCachedParameter(this Node node, string name)
        {
            if (node == null) return null;
            return node.WriteCache as CompiledParameter;
        }
        public static  ResolverRunner<ParameterResolverValue, IParameterResolverContext> GetReadParameterRunner(this Node node, string name)
        {
            if (node == null) return null;
            CompiledParameter cparam = node.GetReadCachedParameter(name);
            if (cparam == null) return null;
            return cparam.Resolver;
        }
        public static ResolverRunner<ParameterResolverValue, IParameterResolverContext> GetWriteParameterRunner(this Node node, string name)
        {
            if (node == null) return null;
            CompiledParameter cparam = node.GetWriteCachedParameter(name);
            if (cparam == null) return null;
            return cparam.Resolver;
        }
        public static ResolverRunner<ParameterResolverValue,IParameterResolverContext> GetParameterRunner(this Node node, NodeExecutionContext execCtx, string name)
        {
            if (node == null) return null;
            if (execCtx == null) return null;
            if(execCtx.Action.Equals(ACTION_READ))
            {
                return node.GetReadParameterRunner(name);
            } else if (execCtx.Action.Equals(ACTION_WRITE))
            {
                return node.GetWriteParameterRunner(name);
            } else
            {
                return null;
            }
        }
        public static void SetReadParameterRunner(this Node node, string name, ResolverRunner<ParameterResolverValue, IParameterResolverContext> runner)
        {
            if (node == null) return;
            CompiledParameter cparam = node.GetReadCachedParameter(name);
            if (cparam == null)
            {
                cparam = new CompiledParameter(name);
                node.ReadCache = cparam;
            };
            
            cparam.Resolver = runner;
        }
        public static void SetWriteParameterRunner(this Node node, string name, ResolverRunner<ParameterResolverValue, IParameterResolverContext> runner)
        {
            if (node == null) return;
            CompiledParameter cparam = node.GetWriteCachedParameter(name);
            if (cparam == null)
            {
                cparam = new CompiledParameter(name);
                node.WriteCache = cparam;
            };

            cparam.Resolver = runner;
        }
        public static void SetParameterRunner(this Node node,NodeExecutionContext execCtx, string name, ResolverRunner<ParameterResolverValue, IParameterResolverContext> runner)
        {
            if (node == null) return;
            if (execCtx == null) return;
            if (execCtx.Action.Equals(ACTION_READ))
            {
                node.SetReadParameterRunner(name, runner);
            } else if (execCtx.Action.Equals(ACTION_WRITE))
            {
                node.SetWriteParameterRunner(name, runner);
            } else
            {
                return;
            }
        }
        #endregion
    }
}
