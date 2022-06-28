namespace Ccf.Ck.Models.NodeSet
{
    public class View : INode
    {
        public string BindingKey
        {
            get;
            set;
        }

        public string SystemPluginName
        {
            get;
            set;
        }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public Settings Settings
        {
            get;
            set;
        }
    }
}