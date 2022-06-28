using System;

namespace Ccf.Ck.Models.NodeSet
{
    public class CustomPlugin : INode
    {
        public const string PLUGIN_PHASE_BEFORE_NODEACTION = "BeforeNodeAction";
        public const string PLUGIN_PHASE_AFTER_NODEACTION = "AfterNodeAction";
        public const string PLUGIN_PHASE_AFTER_NODECHILDREN = "AfterNodeChildren";

        public string CustomPluginName
        {
            get;
            set;
        }

        public string LoadQuery {
            get;
            set;
        }

        public string Query {
            get;
            set;
        }

        public string AssemblyName
        {
            get;
            set;
        }

        public string ClassName
        {
            get;
            set;
        }

        public string PluginConfiguration
        {
            get;
            set;
        }

        public int Executionorder
        {
            get;
            set;
        }

        public bool BeforeNodeAction
        {
            get;
            set;
        }

        public bool AfterNodeAction
        {
            get;
            set;
        }

        public bool AfterNodeChildren
        {
            get;
            set;
        }

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