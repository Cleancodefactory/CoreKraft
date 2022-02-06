using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LibraryAttribute : BaseAttribute
    {
        public LibraryAttribute(string library, LibraryContextFlags context, string contextName = null)
        {
            ContextName = contextName;
            Library = library;
            LibraryContext = context;
        }

        public string ContextName { get; private set; }

        public string Library { get; private set; }

        public LibraryContextFlags LibraryContext { get; private set; }
    }
}
