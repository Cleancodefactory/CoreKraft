using Ccf.Ck.Utilities.Web.BundleTransformations.Primitives;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using static Ccf.Ck.Utilities.Web.BundleTransformations.Primitives.TemplateKraftBundle;

namespace Ccf.Ck.Utilities.Web.BundleTransformations
{
    public class KraftHtml2JsAssocArrayTransformation
    {
        public StringBuilder Process(KraftBundle kraftBundle, Func<StringBuilder, ILogger, StringBuilder> minifyHtml, ILogger logger)
        {
            #region Check validity
            if (kraftBundle == null)
            {
                throw new ArgumentNullException(nameof(kraftBundle));
            }

            #endregion Check validity

            StringBuilder sb = new StringBuilder(1000);
            if (kraftBundle is TemplateKraftBundle templateKraftBundle && templateKraftBundle.TemplateFiles.Count > 0)
            {
                if (templateKraftBundle.ModuleName == null)
                {
                    throw new ArgumentNullException(nameof(templateKraftBundle.ModuleName));
                }
                sb.Append($"\"{templateKraftBundle.ModuleName.ToLowerInvariant()}\"").Append(":{");
                /*  "module1": {
                        "Template1": "html ....",
                        "Template2": "html ...."
                }*/
                bool appendDiv = false;
                foreach(TemplateFile templateFile in templateKraftBundle.TemplateFiles)
                {
                    if (appendDiv) sb.Append(",");
                    else appendDiv = true;

                    sb.Append($"\"{templateFile.TemplateName.ToLowerInvariant()}\":").Append("\"" + /*minifyHtml(*/GetContent(templateFile.PhysicalPath)
                        .Replace("\"", "\\\"")
                        .Replace("'", "\\\'")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Replace("\t", "")/*, logger)*/ + "\"");
                }

                sb.Append("}");
            }
            return sb;
        }

        private StringBuilder GetContent(string fileName)
        {
            string result = string.Empty;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    result = sr.ReadToEnd();
                }
            }
            //result = Regex.Replace(result, "(&)", "&amp;");
            //result = Regex.Replace(result, "(<)", "&lt;");
            //result = Regex.Replace(result, "(>)", "&gt;");
            //result = Regex.Replace(result, "(\r\n|\r|\n)", string.Empty);
            //result = Regex.Replace(result, "(\")", "&quot;");
            return new StringBuilder(result);
        }
    }
}
