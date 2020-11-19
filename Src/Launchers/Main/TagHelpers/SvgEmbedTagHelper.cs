using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ccf.Ck.Launchers.Main.TagHelpers
{
    public class SvgEmbedTagHelper : TagHelper
    {
        private IWebHostEnvironment _WebHostEnvironment;
        public SvgEmbedTagHelper(IWebHostEnvironment env)
        {
            _WebHostEnvironment = env;
        }
        public string RelativePath { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //<svgembed relative-path="/images/landing/share_screen.svg"></svgembed>
            output.Content.SetContent(File.ReadAllText(Path.Combine(_WebHostEnvironment.ContentRootPath, RelativePath)));
        }
    }
}
