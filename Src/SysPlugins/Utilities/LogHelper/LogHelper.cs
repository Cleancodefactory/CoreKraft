using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public static class LogHelper
    {

        /// <summary>
        /// Produces a standard identification line for the location of the error
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="plugin">Must be supplied by the plugin in a human readable but concise manner.</param>
        /// <returns></returns>
        public static string LocationInfo(this IDataLoaderContext ctx, string plugin)
        {
            string module = ctx.ProcessingContext.InputModel.Module;
            string nodepath = ctx.ProcessingContext.InputModel.Nodepath;
            string nodeset = ctx.ProcessingContext.InputModel.NodeSet;

            string currentNode = ctx.CurrentNode.NodeKey;
            string action = ctx.Action;
            string operation = ctx.Operation;

            return $"Location: {module}:{nodeset}/{nodepath}  ({currentNode}|{action}|{operation}) {plugin ?? ""}";

        }

        public static string LocationInfo(this INodePluginContext ctx, string plugin)
        {
            string module = ctx.ProcessingContext.InputModel.Module;
            string nodepath = ctx.ProcessingContext.InputModel.Nodepath;
            string nodeset = ctx.ProcessingContext.InputModel.NodeSet;

            string currentNode = ctx.CurrentNode.NodeKey;
            string action = ctx.Action;
            string operation = ctx.Operation;

            return $"Location: {module}:{nodeset}/{nodepath}  ({currentNode}|{action}|{operation}) {plugin ?? ""}";

        }
    }
}
