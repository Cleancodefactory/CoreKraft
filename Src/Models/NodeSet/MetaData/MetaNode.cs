using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    /// <summary>
    /// TODO MetaNode may contain temporary data which can be confusing if outputed. So together with the finish time recording we have to remove these values as well.
    /// </summary>
    public class MetaNode: IIteratorMeta, IExecutionMeta, IBuiltInExecutionMeta
    {
        #region Nested types
        public class VolatileInfo
        {
            public string DataState { get; set; } = null;
            public string Operation { get; set; } = null;
        }
        
        #endregion


        private readonly MetaRoot _MetaRoot;

        public MetaNode(MetaRoot root, string name) {
            Name = name;
            _MetaRoot = root;
            if (_MetaRoot == null) {
                throw new ArgumentNullException(nameof(root));
            }
            
            Step = root.AddStep(); // Executed this on which step
            Executions = 1;
            if (root.Flags.HasFlag(EMetaInfoFlags.Profile)) {
                ExecutionTimes = new List<DateTime>(1); // Fastest in single execution cases.
                ExecutionTimes.Add(DateTime.UtcNow);
            }
        }

        private MetaNode _Parent;

        public string Name { get; protected set; }
        public int Step { get; protected set; }
        public MetaRoot Root { 
            get {
                return _MetaRoot;
            } 
        }
        public MetaNode GetParent() {
            return _Parent;
        }
        #region Volatile info
        protected VolatileInfo _VolatileInfo { get; } = new VolatileInfo();
        public VolatileInfo GetVolatileInfo() { return _VolatileInfo; }
        #endregion

        #region Execution info
        public int Executions { get; protected set; }
        public List<DateTime> ExecutionTimes { get; protected set; }
        #endregion

        public Dictionary<string, MetaNode> Children { get; protected set; } = new Dictionary<string, MetaNode>();
        public Dictionary<Type, object> Infos { get; set; } = new Dictionary<Type, object>();

        #region IIteratorMeta

        public MetaNode Child(string name) {
            MetaNode node;
            if (Children.ContainsKey(name))
            {
                node = Children[name];
                node.Executions++;
                node._Parent = this;
                if (ExecutionTimes != null) {
                    ExecutionTimes.Add(DateTime.UtcNow);
                }
                return node;
            }
            node = new MetaNode(_MetaRoot, name);
            Children.Add(name, node);
            return node;
        }
        #endregion
        #region IExecutionMeta
        
        public T GetInfo<T>() where T: MetaInfoBase, new() {
            if (Infos.TryGetValue(typeof(T), out var info)) {
                return info as T;
            }
            return default(T);
        }

        public T CreateInfo<T>() where T: MetaInfoBase, new() {
            if (Infos.TryGetValue(typeof(T), out var _info)) {
                var info = _info as T;
                info.AddExecution();
                return info;
            } else {
                var info = new T();
                info.Flags = _MetaRoot.Flags;
                Infos.Add(typeof(T), info);
                return info;
            }
        }



        #endregion
    }
}
