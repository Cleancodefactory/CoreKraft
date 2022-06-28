using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes {
    internal static class InternalExtensions {
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
    }
}
