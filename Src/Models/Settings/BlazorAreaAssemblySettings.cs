using System;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class BlazorAreaAssemblySettings
    {
        public BlazorAreaAssemblySettings()
        {
            BlazorAssemblyNamesCode = new List<string>();
            BlazorInitModuleType = new BlazorInitModuleType();
        }

        public bool IsEnabled { get; set; }

        public bool IsConfigured
        {
            get
            {
                if (BlazorAssemblyNamesCode.Count > 0)
                {
                    return !string.IsNullOrEmpty(BlazorAssemblyNamesCode[0]);
                }
                return false;
            }
        }
        public BlazorInitModuleType BlazorInitModuleType { get; set; }
        public List<string> BlazorAssemblyNamesCode { get; set; }
        public string BlazorStartApplicationWithNamespace { get; set; }
    }
}
