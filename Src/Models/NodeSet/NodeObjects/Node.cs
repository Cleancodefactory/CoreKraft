using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeSet
{
    public class Node : INode
    {
        public Node()
        {
            Views = new List<View>();
            Lookups = new List<Lookup>();
            Children = new List<Node>();
            Parameters = new List<Parameter>();
        }

        #region Private Fields
        private bool _HasRequireAuthenticationSet;
        private bool _RequireAuthentication;
        #endregion

        #region Public Properties
        public string NodeKey
        {
            get;
            set;
        }

        public string DataPluginName
        {
            get;
            set;
        }

        public bool IsList
        {
            get;
            set;
        }

        public bool Trace
        {
            get;
            set;
        }

        public int ExecutionOrder
        {
            get;
            set;
        }
        public string ContinueIf { get; set; }
        public string BreakIf { get; set; }
        public Security Security { get; set; }

        public NodeSet NodeSet
        {
            get;
            set;
        }

        public Read Read
        {
            get;
            set;
        }

        public Write Write
        {
            get;
            set;
        }

        public List<View> Views
        {
            get;
            set;
        }

        public List<Lookup> Lookups
        {
            get;
            set;
        }

        public List<Node> Children
        {
            get;
            set;
        }

        public List<Parameter> Parameters
        {
            get;
            set;
        }

        public Node ParentNode
        {
            get;
            set;
        }

        public bool RequireAuthentication
        {
            get => _RequireAuthentication;
            set
            {
                _HasRequireAuthenticationSet = true;
                _RequireAuthentication = value;
            }
        }

        public bool HasView() => (Views != null && Views.Count > 0);

        public bool HasLookup() => (Lookups != null && Lookups.Count > 0);

        /// <summary>
        /// Collected at first nodeexecutiooncontext execution following the "inheritance" rules
        /// </summary>
        public List<Parameter> CollectedReadParameters { get; set; }
        public object ReadCache { get; set; }
        public List<Parameter> CollectedWriteParameters { get; set; }
        public object WriteCache { get; set; }
        public bool HasValidDataSection(bool iswriteoperation)
        {
            return
                (!iswriteoperation)
                    ? true //Now we support read also for custom plugins without read operation ((Read != null) && (Read.Select != null))
                    : ((Write != null) && ((Write.Insert != null) || (Write.Update != null) || (Write.Delete != null)));
        }

        public void Setup()
        {
            if (string.IsNullOrEmpty(DataPluginName))
            {
                DataPluginName = NodeSet.DataPluginName;
            }
            if (!_HasRequireAuthenticationSet)
            {
                RequireAuthentication = NodeSet.RequireAuthentication;
            }
        }
        #endregion
    }
}