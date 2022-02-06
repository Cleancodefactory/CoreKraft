using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Tools.ActionQueryDocTool
{
    internal class LibsContainer
    {
        public LibsContainer()
        {
            MainLibContainers = new List<LibContainer>();
            NodeLibContainers = new List<LibContainer>();
        }
        public List<LibContainer> MainLibContainers { get; set; }
        public List<LibContainer> NodeLibContainers { get; set; }
    }
}
