using System;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class LoaderProperties
    {
        private Type _ImplementationAsType;
        private Type _InterfaceAsType;

        public string Name { get; set; }

        public string ImplementationAsString { get; set; }

        public Type ImplementationAsType
        {
            get
            {
                return _ImplementationAsType;
            }
            internal set
            {
                _ImplementationAsType = value;
            }
        }

        public string InterfaceAsString { get; set; }

        public Type InterfaceAsType
        {
            get
            {
                return _InterfaceAsType;
            }
            internal set
            {
                _InterfaceAsType = value;
            }
        }

        public bool Default { get; set; }

        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
