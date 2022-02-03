﻿using System;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterAttribute : BaseAttribute
    {
        public ParameterAttribute(int order, string name, string documentation, ParameterType paramType = ParameterType.String)
        {
            Order = order;
            Name = name;
            Documentation = documentation;
            ParamType = paramType;
        }

        public int Order { get; private set; }

        public string Name { get; private set; }

        public string Documentation { get; private set; }

        public ParameterType ParamType { get; private set; }
    }
}