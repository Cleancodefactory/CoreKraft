using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ResultAttribute : BaseAttribute
    {
        public ResultAttribute(string documentation, ResultType resultType)
        {
            Documentation = documentation;
            ResultType = resultType;
        }

        public string Documentation { get; private set; }

        public new ResultType ResultType { get; private set; }
    }
}
