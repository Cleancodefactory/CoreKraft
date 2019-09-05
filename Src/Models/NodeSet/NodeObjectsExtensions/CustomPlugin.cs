using System;

namespace Ccf.Ck.Models.NodeSet
{
    public partial class CustomPlugin : INode
    {
        //public const string PLUGIN_PHASE_BEFORE_SQL = "BeforeSql";
        //public const string PLUGIN_PHASE_AFTER_SQL = "AfterSql";
        //public const string PLUGIN_PHASE_AFTER_CHILDREN = "AfterChildren";

        public const string PLUGIN_PHASE_BEFORE_NODEACTION = "BeforeNodeAction";
        public const string PLUGIN_PHASE_AFTER_NODEACTION = "AfterNodeAction";
        public const string PLUGIN_PHASE_AFTER_NODECHILDREN = "AfterNodeChildren";


        /// <summary>
        /// The phase in which the plugin should be executed
        /// </summary>
        public string ExecutionBehavior //TODO: deal with the behavior
        {
            set
            {
                if (value.Equals(PLUGIN_PHASE_BEFORE_NODEACTION, StringComparison.CurrentCultureIgnoreCase))
                {
                    BeforeNodeAction = true;
                }
                else if (value.Equals(PLUGIN_PHASE_AFTER_NODEACTION, StringComparison.CurrentCultureIgnoreCase))
                {
                    AfterNodeAction = true;
                }
                else
                {
                    AfterNodeChildren = true;
                }
            }
        }
    }
}