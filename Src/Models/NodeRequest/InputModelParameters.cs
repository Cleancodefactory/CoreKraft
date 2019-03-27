using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings;
using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeRequest
{
    public class InputModelParameters
    {
        public string Module { get; set; }
        public string Nodeset { get; set; }
        public string Nodepath { get; set; }
        public string BindingKey { get; set; }
        public bool IsWriteOperation { get; set; }
        public ELoaderType LoaderType { get; set; }
        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; set; }

        public Dictionary<string, object> QueryCollection { get; set; }
        public Dictionary<string, object> HeaderCollection { get; set; }
        public Dictionary<string, object> FormCollection { get; set; }

        public Dictionary<string, object> Server { get; set; }

        public ISecurityModel SecurityModel { get; set; }

        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
