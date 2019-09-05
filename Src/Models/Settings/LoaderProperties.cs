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
                //if (_ImplementationAsType == null)
                //{
                //    _ImplementationAsType = Type.GetType(ImplementationAsString) ?? throw new NullReferenceException(nameof(ImplementationAsString));
                //}

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
                //if (_InterfaceAsType == null)
                //{
                //    _InterfaceAsType = Type.GetType(InterfaceAsString) ?? throw new NullReferenceException(nameof(InterfaceAsString));
                //}

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
