using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.InternalCalls
{
    public class DirectCallLib<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                //case nameof(PngFromImage):
                //   return PngFromImage;
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Basic Image library (no symbols)", null);
        }

        private List<object> _disposables = new List<object>();
        public void ClearDisposables()
        {
            lock (_LockObject)
            {
                for (int i = 0; i < _disposables.Count; i++)
                {
                    if (_disposables[i] is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                }
                _disposables.Clear();
            }
        }
        #endregion

        #region Helpers
        private static Regex reAddress = new Regex(@"([a-zA-Z][a-zA-Z0-9\-\.\_]*)/([a-zA-Z][a-zA-Z0-9\-\.\_]*)(?:([a-zA-Z][a-zA-Z0-9\-\.\_]*))?",RegexOptions.Compiled);
        /// <summary>
        /// Parses the address into module, nodeset, nodepath
        /// 
        /// </summary>
        /// <param name="address">Syntax is: module/nodeset/nodepath</param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool ParseCallAddress(string address, InputModel input)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Functions

        public ParameterResolverValue Test(HostInterface ctx, ParameterResolverValue[] args)
        {
            InputModel inp = new InputModel();
            ReturnModel ret = null;
            throw new NotImplementedException();
            

            //DirectCallService.Instance.Call()
        }

        #endregion

    }
}
