using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Views.Html
{
    public class HtmlViewSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}