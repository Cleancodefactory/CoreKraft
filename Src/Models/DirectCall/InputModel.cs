using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.DirectCall
{
    public class InputModel
    {
        public string Module { get; set; }
        public string Nodeset { get; set; }
        public string Nodepath { get; set; }
        public bool IsWriteOperation { get; set; }
        public Dictionary<string, object> QueryCollection { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
