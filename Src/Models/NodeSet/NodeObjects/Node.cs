using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeSet
{
    public class Node : INode
    {
        public Node()
        {
            _Views = new List<View>();
            _Lookups = new List<Lookup>();
            _Children = new List<Node>();
            _Parameters = new List<Parameter>();
        }

        #region Private Fields
        private List<View> _Views;
        private List<Lookup> _Lookups;
        private List<Node> _Children;
        private List<Parameter> _Parameters;
        private bool _HasRequireAuthenticationSet;
        private bool _RequireAuthentication;
        private Read _Read;
        private Write _Write;
        private int _ExecutionOrder;
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

        /// <summary>
        /// Takes care if the ExecutionOrder is updated after Read or Write
        /// </summary>
        public int ExecutionOrder
        {
            get
            {
                return _ExecutionOrder;
            }
            set
            {
                _ExecutionOrder = value;
                if (_ExecutionOrder != 0) //Update only when different
                {
                    if (_Read != null)
                    {
                        _Read.ExecutionOrder = _ExecutionOrder;
                    }
                    if (_Write != null)
                    {
                        _Write.ExecutionOrder = _ExecutionOrder;
                    }
                }
            }
        }

        public NodeSet NodeSet
        {
            get;
            set;
        }

        /// <summary>
        /// If the read section is updated after ExecutionOrder apply changes
        /// </summary>
        public Read Read
        {
            get
            {
                return _Read;
            }
            set
            {
                _Read = value;
                if (_ExecutionOrder != 0)
                {
                    if (_Read.ExecutionOrder == 0) //override default only
                    {
                        _Read.ExecutionOrder = ExecutionOrder;
                    }
                }
            }
        }

        /// <summary>
        /// If the write section is updated after ExecutionOrder apply changes
        /// </summary>
        public Write Write
        {
            get
            {
                return _Write;
            }
            set
            {
                _Write = value;
                if (_ExecutionOrder != 0)
                {
                    if (_Write.ExecutionOrder == 0) //override default only
                    {
                        _Write.ExecutionOrder = ExecutionOrder;
                    }
                }
            }
        }

        public List<View> Views
        {
            get
            {
                return _Views;
            }

            set
            {
                _Views = value;
            }
        }

        public List<Lookup> Lookups
        {
            get
            {
                return _Lookups;
            }

            set
            {
                _Lookups = value;
            }
        }

        public List<Node> Children
        {
            get
            {
                return _Children;
            }

            set
            {
                _Children = value;
            }
        }

        public List<Parameter> Parameters
        {
            get
            {
                return _Parameters;
            }

            set
            {
                _Parameters = value;
            }
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


        public List<Parameter> CollectedReadParameters { get; set; }
        public List<Parameter> CollectedWriteParameters { get; set; }

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