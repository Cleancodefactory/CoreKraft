using Ccf.Ck.Libs.ActionQuery;
using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class ActionQueryTrace
    {
        public ActionQueryTrace(IEnumerable<Instruction> program, int steplimit)
        {
            _program = program.ToArray();
            _steps = new Queue<ActionQueryStep>(steplimit+1);
        }

        #region Raw data
        private int _steplimit = 10;
        private Instruction[] _program;

        public IEnumerable<Instruction> Program
        {
            get
            {
                return _program;
            }
        }

        private Queue<ActionQueryStep> _steps = null;

        public void AddStep(ActionQueryStep step)
        {
            _steps.Enqueue(step);
            if (_steps.Count > _steplimit)
            {
                _steps.Dequeue();
            }
        }
        public IEnumerable<ActionQueryStep> Steps
        {
            get
            {
                return _steps.Reverse().ToArray();
            }
        }
        #endregion

        #region report
        

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var steps = Steps;
            if (steps != null && steps.Count() > 0) {
                sb.AppendLine("AQ detailed trace information ===============");
                sb.AppendLine($"Last {Steps.Count()} steps are listed:");
                for (var i = Steps.Count() -1; i >= 0; i--)
                {
                    sb.AppendLine($"Step {i}\n{Steps.ToString()}");
                }
                
            }
            return sb.ToString();
        }
        #region static helpers

        public static string ExceptionToString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            Exception current = ex;
            int level = 0;
            while (current != null)
            {
                if (current is ActionQueryException<ParameterResolverValue> acex)
                {
                    sb.AppendLine($" --- Exception level {level++}: {current.Message}");
                    sb.AppendLine($"PC={acex.Pc}");
                    sb.AppendLine($"Instruction: {acex.Instruction}");
                    var stack = acex.AQStack;
                    if (stack != null)
                    {
                        sb.AppendLine($" === stack dump === \n {string.Join("\n", stack.Select(s => s.ToString()))}\n === end of stack dump ==");
                    }
                    current = current.InnerException;
                } 
                else
                {
                    sb.AppendLine($" --- Exception level {level++}: {current.Message}");
                    current = current.InnerException;
                }
            }
            return sb.ToString();
        }
        #endregion
        #endregion
    }
}
