using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    public class BaseAttribute: Attribute
    {
        public enum ParameterType
        {
            Int,
            Double,
            Bool,
            String,
            Object,
            Null
        }

        public enum ResultType
        {
            Int,
            Double,
            Bool,
            String,
            Json,
            Dict
        }

    }
}
