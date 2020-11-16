using Ccf.Ck.Utilities.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Models.Settings
{
    public class RequestRecorderSetting
    {
        public string ImplementationAsString { get; set; }
        public string InterfaceAsString { get; set; }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrEmpty(ImplementationAsString) && !string.IsNullOrEmpty(InterfaceAsString);
            }
        }
    }
}
