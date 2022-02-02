using System;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    public class DocToolAttribute : Attribute
    {
        public DocToolAttribute(string summary, string docLink, string label)
        {
            this.label = label;
            this.summary = summary;
            this.docLink = docLink;
        }

        public string summary { get; private set; }

        public string docLink { get; private set; }

        public string label { get; private set; }
    }
}
