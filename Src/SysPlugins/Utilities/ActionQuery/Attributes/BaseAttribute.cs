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
        public enum TypeEnum
        {
            Int = 0x010,
            Double = 0x020,
            Bool = 0x040,
            String = 0x080,
            Json = 0x010,
            Dict = 0x020,
            List = 0x040,
            Null = 0x080,
            Object = 0x100
        }
    }
}
