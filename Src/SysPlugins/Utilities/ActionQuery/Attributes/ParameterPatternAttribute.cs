using System;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterPatternAttribute : BaseAttribute
    {
        public ParameterPatternAttribute(int order, string name, string documentation, ParameterType firstType, ParameterType secondType, ParameterType thirdType = ParameterType.Null)
        {
            Order = order;
            Name = name;
            Documentation = documentation;
            FirstType = firstType;
            SecondType = secondType;
            ThirdType = thirdType;
        }

        public int Order { get; private set; }

        public string Name { get; private set; }

        public string Documentation { get; private set; }

        public ParameterType FirstType { get; private set; }
        public ParameterType SecondType { get; private set; }
        public ParameterType ThirdType { get; private set; }
    }
}
