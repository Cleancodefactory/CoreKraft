using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    public class BaseAttribute : Attribute
    {
        [Flags]
        public enum TypeFlags
        {
            Varying = 0x000,
            Int     = 0x001,
            Double  = 0x002,
            Bool    = 0x004,
            String  = 0x008,
            Json    = 0x010,
            Dict    = 0x020,
            List    = 0x040,
            Null    = 0x080,
            Object  = 0x100,
            Error   = 0x200
        }

        /// <summary>
        /// The context in which the library can be used (e.g. data loader, node custom plugin and any others in future
        /// </summary>
        [Flags]
        public enum LibraryContextFlags
        {
            Main        = 0x0001,
            Node        = 0x0002,
            MainNode    = 0x0003,
            Unspecified = 0x0004 // For temporary marking of contexts in development

        }
    }
}
