using Ccf.Ck.Libs.ActionQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class ActionQueryTrace
    {
        #region Raw data
        private Instruction[] _program;

        public IEnumerable<Instruction> Program
        {
            get
            {
                return _program;
            }
        }

        #endregion
    }
}
