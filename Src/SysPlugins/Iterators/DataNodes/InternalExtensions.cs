using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
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
                    OPERATION_INSERT => node.Write?.Insert.ExecutionOrder ?? ord,
                    OPERATION_UPDATE => node.Write?.Update.ExecutionOrder ?? ord,
                    OPERATION_DELETE => node.Write?.Delete.ExecutionOrder ?? ord,
                    _ => ord
                };
            }
            return ord;
        }
        #endregion
    }
}
