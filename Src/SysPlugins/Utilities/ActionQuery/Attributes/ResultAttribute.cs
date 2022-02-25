using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ResultAttribute : BaseAttribute
    {
        public ResultAttribute(string documentation, TypeFlags typeEnum)
        {
            Documentation = documentation;
            TypeEnum = typeEnum;
        }

        public string Documentation { get; private set; }

        public TypeFlags TypeEnum { get; private set; }
    }
}
