using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.Settings
{
    public class OverrideModuleSetting
    {
        public string ModuleName { get; set; }
        public List<Loader> Loaders { get; set; }
        public OverrideModuleSetting()
        {
            Loaders = new List<Loader>();
        }
    }

    public class Loader
    {
        public string LoaderName { get; set; }
        public Dictionary<string, string> CustomSettings { get; set; }
        public Loader()
        {
            CustomSettings = new Dictionary<string, string>();
        }
    }
}
