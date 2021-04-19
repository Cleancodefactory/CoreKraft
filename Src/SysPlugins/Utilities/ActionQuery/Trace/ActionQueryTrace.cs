using Ccf.Ck.Libs.ActionQuery;
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
    }
}
