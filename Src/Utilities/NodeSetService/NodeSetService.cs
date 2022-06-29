using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Models.NodeSet;
using NodeSetModel = Ccf.Ck.Models.NodeSet.NodeSet;
using Ccf.Ck.Models.Settings;

namespace Ccf.Ck.Utilities.NodeSetService
{
    public class NodeSetService : INodeSetService
    {
        #region Private Fields
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        private readonly ICachingService _CachingService;
        #endregion

        #region Constructors
        public NodeSetService(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, ICachingService cachingService)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _CachingService = cachingService ?? throw new NullReferenceException(nameof(cachingService));
        }
        #endregion

        #region Public Methods
        public LoadedNodeSet LoadNodeSet(string module, string treeNodesName, string nodeName)
        {
            LoadedNodeSet loaderContext = new LoadedNodeSet();
            //find the node definition in the directory specified
            //load the definition
            //parse the definition and populate into the models
            NodeSetModel nodeSet = null;
            string NODESET_DIRNAME = "NodeSets";
            string cacheKey = $"NodeSet_{module}_{treeNodesName}";
            nodeSet = _CachingService.Get<NodeSetModel>(cacheKey);
            if (nodeSet == null)
            {
                nodeSet = new NodeSetModel();
                string nodeSetDir = Path.Combine(_KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(module), module, NODESET_DIRNAME, treeNodesName);
                string nodeSetFile = Path.Combine(nodeSetDir, "Definition.json");

                if (!File.Exists(nodeSetFile))
                {
                    throw new FileNotFoundException($"The requested file: {nodeSetFile} was not found");
                }

                PhysicalFileProvider fileProvider = new PhysicalFileProvider(nodeSetDir);

                using (StreamReader file = File.OpenText(nodeSetFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    RootObject result = (RootObject)serializer.Deserialize(file, typeof(RootObject));
                    nodeSet = result.NodeSet;
                };

                nodeSet.Name = treeNodesName;

                ProcessNodes(nodeSet.Root, nodeSetFile, nodeSet);
                _CachingService.Insert(cacheKey, nodeSet, fileProvider.Watch("**/*.*"));
            }

            loaderContext.NodeSet = nodeSet;
            loaderContext.StartNode =
                (!string.IsNullOrEmpty(nodeName))
                    ? FindNode(nodeSet.Root.Children, ParseNodePath(nodeName), n => n.Children, n => n.NodeKey.Trim())
                    : nodeSet.Root;

            return loaderContext;
        }
        #endregion

        #region Helpers And Utilities

        private void ProcessNodes(Node startNode, string nodeSetFile, NodeSetModel nodeSet)
        {            
            if (startNode.Children != null)
            {
                startNode.Children = new List<Node>(startNode.Children.OrderBy(childNode => childNode.ExecutionOrder));
            }

            if (startNode.Views != null)
            {
                startNode.Views = new List<View>(startNode.Views.OrderBy(viewDefinition => viewDefinition.ExecutionOrder));
            }

            LoadCustomPluginQueries(nodeSetFile, startNode);

            ReorderPlugins(startNode.Read);
            ReorderPlugins(startNode.Write);

            LoadQueryFiles(nodeSetFile, startNode);
            LoadFileContent(nodeSetFile, startNode);
            startNode.NodeSet = nodeSet;
            startNode.Setup();

            foreach (Node node in startNode.Children)
            {
                node.ParentNode = startNode;
                ProcessNodes(node, nodeSetFile, nodeSet);
            }
        }

        private void ReorderPlugins(OperationBase operation)
        {
            if (operation?.CustomPlugins != null)
            {
                operation.BeforeNodeActionPlugins =
                    new List<CustomPlugin>(operation.CustomPlugins.OrderBy(p => p.Executionorder).Where(p => p.BeforeNodeAction == true));
                operation.AfterNodeActionPlugins =
                    new List<CustomPlugin>(operation.CustomPlugins.OrderBy(p => p.Executionorder).Where(p => p.AfterNodeAction == true));
                operation.AfterNodeChildrenPlugins =
                    new List<CustomPlugin>(operation.CustomPlugins.OrderBy(p => p.Executionorder).Where(p => p.AfterNodeChildren == true));

                operation.CustomPlugins.Clear();
            }
        }

        private void LoadCustomPluginQueries(string fileName, Node node) {
            // This method must be called before transferring the custom plugins to the special collections (before, after, after children)
            if (node != null) {
                if (node.Read != null && node.Read.CustomPlugins != null) {
                    foreach (CustomPlugin plugin in node.Read.CustomPlugins) {
                        if (!string.IsNullOrWhiteSpace(plugin.LoadQuery)) {
                            ReadNodeQueryFile(fileName, plugin);
                        }
                    }
                }
                if (node.Write != null && node.Write.CustomPlugins != null) {
                    foreach (CustomPlugin plugin in node.Write.CustomPlugins) {
                        if (!string.IsNullOrWhiteSpace(plugin.LoadQuery)) {
                            ReadNodeQueryFile(fileName, plugin);
                        }
                    }
                }
            }
        }
        private void ReadNodeQueryFile(string fileName, CustomPlugin plugin) {
            string filePath = CalculateFilePath(fileName, plugin.LoadQuery);
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"The requested file in loadquery field of custom plugin: {filePath} was not found");
            }

            plugin.Query = File.ReadAllText(filePath);
            plugin.LoadQuery = null;
        }

        private void LoadQueryFiles(string fileName, Node node)
        {
            if (node != null)
            {
                if (node.Read != null)
                {
                    if (node.Read.Select != null && node.Read.Select.HasLoadQuery())
                    {
                        ReadQueryFile(fileName, node.Read.Select);
                    }
                    else if (node.Read.New != null && node.Read.New.HasLoadQuery())
                    {
                        ReadQueryFile(fileName, node.Read.New);
                    }
                }
                if (node.Write != null)
                {
                    if (node.Write.Insert != null && node.Write.Insert.HasLoadQuery())
                    {
                        ReadQueryFile(fileName, node.Write.Insert);
                    }
                    if (node.Write.Update != null && node.Write.Update.HasLoadQuery())
                    {
                        ReadQueryFile(fileName, node.Write.Update);
                    }
                    if (node.Write.Delete != null && node.Write.Delete.HasLoadQuery())
                    {
                        ReadQueryFile(fileName, node.Write.Delete);
                    }
                }
                if (node.HasLookup())
                {
                    foreach (Lookup lookup in node.Lookups)
                    {
                        if (lookup.HasLoadQuery()) ReadQueryFile(fileName, lookup);
                    }
                }
            }
        }
        private void ReadQueryFile(string fileName, ActionBase action)
        {
            string filePath = CalculateFilePath(fileName, action.LoadQuery);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The requested file: {filePath} was not found");
            }

            action.Query = File.ReadAllText(filePath);
            action.LoadQuery = null;
        }



        private void LoadFileContent(string fileName, Node node)
        {
            if (node != null)
            {
                if (node.Read != null && node.Read.Select != null && node.Read.Select.HasFile())
                {
                    ReadFile(fileName, node.Read.Select);
                }
                if (node.Write != null)
                {
                    if (node.Write.Insert != null && node.Write.Insert.HasFile())
                    {
                        ReadFile(fileName, node.Write.Insert);
                    }
                    if (node.Write.Update != null && node.Write.Update.HasFile())
                    {
                        ReadFile(fileName, node.Write.Update);
                    }
                    if (node.Write.Delete != null && node.Write.Delete.HasFile())
                    {
                        ReadFile(fileName, node.Write.Delete);
                    }
                }
                if (node.HasLookup())
                {
                    foreach (Lookup lookup in node.Lookups)
                    {
                        if (lookup.HasFile()) ReadFile(fileName, lookup);
                    }
                }
            }
        }

        private void ReadFile(string fileName, ActionBase action)
        {
            string filePath = CalculateFilePath(fileName, action.File);

            if (action.IsTypeOf("sql"))
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The requested file: {filePath} was not found");
                }

                action.Query = File.ReadAllText(filePath);
                action.File = null;
            }
        }

        private string CalculateFilePath(string fullPath, string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(fullPath), fileName);
        }

        private List<string> ParseNodePath(params string[] strs)
        {
            foreach (string str in strs)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var r = new List<string>();
                    r.AddRange(from s in str.Split('.') select s.Trim());
                    return r;
                }
            }
            return null;
        }

        private T FindNode<T>(
            List<T> start
            , List<string> path
            , Func<T, List<T>> getchildren
            , Func<T, string> getkey
            , int nPos = 0) where T : Node
        {
            if (start == null || start.Count == 0) return null;
            if (path == null || path.Count == 0 || nPos >= path.Count) return null;
            string currentNodeKey = path[nPos];
            T t;
            if (string.IsNullOrWhiteSpace(currentNodeKey))
            {
                t = start.Count > 0 ? start.FirstOrDefault() : null;
            }
            else
            {
                t = start.Find(node => node.NodeKey.Equals(currentNodeKey, StringComparison.CurrentCultureIgnoreCase));
            }
            if (t == null) return null;
            if (nPos == path.Count - 1)
            {
                // End
                return t;
            }

            return FindNode(getchildren(t), path, getchildren, getkey, nPos + 1);
        }
        #endregion
    }
}