using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public interface IActionQueryLibrary<HostInterface> where HostInterface: class
    {
        HostedProc<HostInterface> GetProc(string name);

        SymbolSet GetSymbols();
    }
}
