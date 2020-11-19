using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ccf.Ck.Launchers.Main.TagHelpers
{
    [HtmlTargetElement("svgembed")]
    public class SvgEmbedTagHelper : TagHelper
    {
        private IWebHostEnvironment _WebHostEnvironment;
        private string _RelativePath;

        public SvgEmbedTagHelper(IWebHostEnvironment env)
        {
            _WebHostEnvironment = env;
        }
        public string RelativePath
        {
            get {
                return _RelativePath.TrimStart('/').TrimStart('\\'); 
            }
            set { _RelativePath = value; }
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //<svgembed relative-path="/images/landing/share_screen.svg"></svgembed>
            output.TagName = null;
            output.Content.SetHtmlContent(File.ReadAllText(Path.Combine(_WebHostEnvironment.WebRootPath, RelativePath)));
        }
    }
}
