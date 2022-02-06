using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Tools.ActionQueryDocTool
{
    internal class LibContainer
    {
        public LibContainer(string libName)
        {
            MethodAttributes = new List<MethodAttributes>();
        }
        public List<MethodAttributes> MethodAttributes { get; set; }
        public string LibName { get; private set; }
    }
}
