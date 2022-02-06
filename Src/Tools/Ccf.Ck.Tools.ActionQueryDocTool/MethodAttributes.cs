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
        private const string INDENTATION = "  ";
        internal FunctionAttribute FunctionAttribute { get; set; }
        internal List<ParameterAttribute> ParameterAttributes { get; set; }
        internal List<ParameterPatternAttribute> ParameterPatternAttributes { get; set; }
        internal ResultAttribute ResultAttribute { get; set; }

        internal string CompileDocumentation()
        {
            StringBuilder result = new StringBuilder(1000);
            result.AppendLine(FunctionAttribute.Documentation + " (" + FunctionAttribute.Library + ")");
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
                    result.AppendLine(INDENTATION + "Desc: " + parameter.Documentation);
                    result.AppendLine(INDENTATION + "Type: " + parameter.ParamType.ToString());
                }
            }

            if (ParameterPatternAttributes != null)
            {
                ParameterPatternAttributes.Sort((p, q) => p.Order.CompareTo(q.Order));
                foreach (ParameterPatternAttribute parameterPattern in ParameterPatternAttributes)
                {
                    result.AppendLine("Parameter: " + parameterPattern.Name);
                    result.AppendLine(INDENTATION + "Desc: " + parameterPattern.Documentation);
                    result.AppendLine(INDENTATION + "First-Type: " + parameterPattern.FirstType.ToString());
                    if (parameterPattern.SecondType != BaseAttribute.TypeFlags.Null)
                    {
                        result.AppendLine(INDENTATION + "Second-Type: " + parameterPattern.SecondType.ToString());
                    }
                }
            }

            if (ResultAttribute != null)
            {
                result.AppendLine("Returns: " + ResultAttribute.Documentation);
                result.AppendLine(INDENTATION + "Type: " + ResultAttribute.TypeEnum.ToString());
            }

            return result.ToString();
        }
    }
}
