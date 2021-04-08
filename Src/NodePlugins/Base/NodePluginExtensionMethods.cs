using System.Collections.Generic;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.NodePlugins.Base
{
    public static class NodePluginExtensionMethods
    {
        public static void SetUnchanged(this Dictionary<string, object> data)
        {
            data[STATE_PROPERTY_NAME] = STATE_PROPERTY_UNCHANGED;
        }
        public static void SetUpdated(this Dictionary<string, object> data)
        {
            data[STATE_PROPERTY_NAME] = STATE_PROPERTY_UPDATE;
        }
        public static void SetNew(this Dictionary<string, object> data)
        {
            data[STATE_PROPERTY_NAME] = STATE_PROPERTY_INSERT;
        }
        public static void SetDeleted(this Dictionary<string, object> data)
        {
            data[STATE_PROPERTY_NAME] = STATE_PROPERTY_DELETE;
        }
    }
}
