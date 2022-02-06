using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FunctionAttribute : BaseAttribute
    {
        public FunctionAttribute(string functionname, string documentation)
        {
            FunctionName = functionname;
            Documentation = documentation;
        }

        public string Documentation { get; private set; }

        public string FunctionName { get; private set; }
    }
}
