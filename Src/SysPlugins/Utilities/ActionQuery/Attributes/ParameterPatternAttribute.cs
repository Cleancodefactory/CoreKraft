using System;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterPatternAttribute : BaseAttribute
    {
        public ParameterPatternAttribute(int order, string name, string documentation, TypeFlags firstType, TypeFlags secondType = TypeFlags.Null)
        {
            Order = order;
            Name = name;
            Documentation = documentation;
            FirstType = firstType;
            SecondType = secondType;
        }

        public int Order { get; private set; }

        public string Name { get; private set; }

        public string Documentation { get; private set; }

        public TypeFlags FirstType { get; private set; }
        public TypeFlags SecondType { get; private set; }
    }
}
