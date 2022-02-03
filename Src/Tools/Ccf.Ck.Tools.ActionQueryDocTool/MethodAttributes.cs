using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Tools.ActionQueryDocTool
{
    internal class MethodAttributes
    {
        internal FunctionAttribute FunctionAttribute { get; set; }
        internal List<ParameterAttribute> ParameterAttributes { get; set; }
        internal List<ParameterPatternAttribute> ParameterPatternAttributes { get; set; }
        internal ResultAttribute ResultAttribute { get; set; }

        internal string CompileDocumentation()
        {
            StringBuilder result = new StringBuilder(1000);
            result.AppendLine(FunctionAttribute.Documentation + "(" + FunctionAttribute.Library + ")");
            return result.ToString();
        }

        internal object CompileDetail()
        {
            StringBuilder result = new StringBuilder(1000);
            if (ParameterAttributes != null)
            {
                ParameterAttributes.Sort((p, q) => p.Order.CompareTo(q.Order));
                foreach (ParameterAttribute parameter in ParameterAttributes)
                {
                    result.AppendLine("Parameter: " + parameter.Name);
                    result.AppendLine("  Desc: " + parameter.Documentation);
                    result.AppendLine("  Type: " + parameter.ParamType.ToString());
                }
            }

            if (ParameterPatternAttributes != null)
            {
                ParameterPatternAttributes.Sort((p, q) => p.Order.CompareTo(q.Order));
                foreach (ParameterPatternAttribute parameterPattern in ParameterPatternAttributes)
                {
                    result.AppendLine("Parameter: " + parameterPattern.Name);
                    result.AppendLine("  Desc: " + parameterPattern.Documentation);
                    result.AppendLine("  First-Type: " + parameterPattern.FirstType.ToString());
                    if (parameterPattern.SecondType != BaseAttribute.ParameterType.Null)
                    {
                        result.AppendLine("  Second-Type: " + parameterPattern.SecondType.ToString());
                    }
                    if (parameterPattern.ThirdType != BaseAttribute.ParameterType.Null)
                    {
                        result.AppendLine("  Third-Type: " + parameterPattern.ThirdType.ToString());
                    }
                }
            }

            if (ResultAttribute != null)
            {
                result.AppendLine("Returns: " + ResultAttribute.Documentation);
                result.AppendLine("Type: " + ResultAttribute.ResultType.ToString());
            }

            return result.ToString();
        }
    }
}
