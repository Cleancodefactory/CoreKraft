using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeSet
{
    public abstract class OperationBase
    {
        public OperationBase()
        {
            _CustomPlugins = new List<CustomPlugin>();
            _BeforeNodeActionPlugins = new List<CustomPlugin>();
            _AfterNodeActionPlugins = new List<CustomPlugin>();
            _AfterNodeChildrenPlugins = new List<CustomPlugin>();
            Parameters = new List<Parameter>();
        }

        #region Private Fields
        private List<CustomPlugin> _CustomPlugins;
        private List<CustomPlugin> _BeforeNodeActionPlugins;
        private List<CustomPlugin> _AfterNodeActionPlugins;
        private List<CustomPlugin> _AfterNodeChildrenPlugins;
        #endregion

        #region Public Properties
        public List<Parameter> Parameters
        {
            get;
            set;
        }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public List<CustomPlugin> CustomPlugins
        {
            get
            {
                return _CustomPlugins;
            }

            set
            {
                _CustomPlugins = value;
            }
        }

        public List<CustomPlugin> BeforeNodeActionPlugins
        {
            get
            {
                return _BeforeNodeActionPlugins;
            }

            set
            {
                _BeforeNodeActionPlugins = value;
            }
        }

        public List<CustomPlugin> AfterNodeActionPlugins
        {
            get
            {
                return _AfterNodeActionPlugins;
            }

            set
            {
                _AfterNodeActionPlugins = value;
            }
        }

        public List<CustomPlugin> AfterNodeChildrenPlugins
        {
            get
            {
                return _AfterNodeChildrenPlugins;
            }

            set
            {
                _AfterNodeChildrenPlugins = value;
            }
        }
        #endregion
    }
}