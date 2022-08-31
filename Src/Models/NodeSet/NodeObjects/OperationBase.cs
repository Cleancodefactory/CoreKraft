using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeSet
{
    public abstract class OperationBase
    {
        public OperationBase()  { }

        

        #region Public Properties
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        

        public int ExecutionOrder 
        {
            get;
            set;
        }

        public List<CustomPlugin> CustomPlugins { get; set; }  = new List<CustomPlugin>();
        
        public List<CustomPlugin> BeforeNodePlugins { get; set; } = new List<CustomPlugin>();
        
        public List<CustomPlugin> BeforeNodeActionPlugins { get; set; } = new List<CustomPlugin>();

        public List<CustomPlugin> AfterNodeActionPlugins { get; set; } = new List<CustomPlugin>();

        public List<CustomPlugin> AfterNodeChildrenPlugins { get; set; } = new List<CustomPlugin>();
        
        #endregion
    }
}