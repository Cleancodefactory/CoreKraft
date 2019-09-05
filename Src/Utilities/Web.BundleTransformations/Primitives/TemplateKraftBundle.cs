using System.Collections.Generic;

namespace Ccf.Ck.Utilities.Web.BundleTransformations.Primitives
{
    public class TemplateKraftBundle : KraftBundle
    {
        public List<TemplateFile> TemplateFiles { get; set; }
        public string ModuleName { get; set; }

        public TemplateKraftBundle()
        {
            TemplateFiles = new List<TemplateFile>();
        }

        public struct TemplateFile
        {
            public string PhysicalPath { get; set; }
            public string TemplateName { get; set; }
        }
    }
}
