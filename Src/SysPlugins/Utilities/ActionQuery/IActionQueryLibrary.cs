using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public interface IActionQueryLibrary
    {
        Func<ParameterResolverValue[], ParameterResolverValue> GetProc(string name);
    }
}
