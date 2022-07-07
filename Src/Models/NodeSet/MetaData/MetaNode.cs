using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class MetaNode: IIteratorMeta, IExecutionMeta {

        private MetaRoot _metaRoot;

        public MetaNode(MetaRoot root, string name) {
            Name = name;
            if (_metaRoot == null) {
                throw new ArgumentNullException(nameof(root));
            }
            _metaRoot = root;
            Step = root.AddStep(); // Executed this on which step
        }

        public string Name { get; protected set; }
        public int Step { get; protected set; }
        public Dictionary<string, MetaNode> Children { get; protected set; }
        private Dictionary<Type, object> Infos { get; set; } = new Dictionary<Type, object>();

        #region IIteratorMeta

        public MetaNode Child(string name) {
            if (Children.ContainsKey(name)) return Children[name];
            var node = new MetaNode(_metaRoot, name);
            Children.Add(name, node);
            
            return node;
        }
        #endregion
        #region IExecutionMeta
        public T GetInfo<T>() where T: MetaInfoBase {
            if (Infos.TryGetValue(typeof(T), out var info)) {
                return info as T;
            }
            return default(T);
        }

        public T SetInfo<T>(T info) where T: MetaInfoBase {
            if (Infos.TryGetValue(typeof(T), out var _info)) {
                return _info as T;
            } else {
                if (info != null) {
                    info.Flags = _metaRoot.Flags;
                }
                Infos.Add(typeof(T), info);
                return info;
            }
        }



        #endregion
    }
}
