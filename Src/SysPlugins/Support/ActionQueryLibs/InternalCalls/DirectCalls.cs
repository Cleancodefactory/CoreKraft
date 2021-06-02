using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Ccf.Ck.Models.NodeRequest;

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
        private static Regex reAddress = new Regex(@"^([a-zA-Z][a-zA-Z0-9\-\.\_]*)/([a-zA-Z][a-zA-Z0-9\-\.\_]*)(?:([a-zA-Z][a-zA-Z0-9\-\.\_]*))?$",RegexOptions.Compiled);
        /// <summary>
        /// Parses the address into module, nodeset, nodepath
        /// 
        /// </summary>
        /// <param name="address">Syntax is: module/nodeset/nodepath</param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool ParseCallAddress(string address, InputModel input)
        {
            Match m = reAddress.Match(address);
            if (m.Success)
            {
                if (m.Groups[0].Success)
                {
                    input.Module = m.Groups[0].Value;
                    if (m.Groups[1].Success)
                    {
                        input.Nodeset = m.Groups[1].Value;
                        if (m.Groups[2].Success)
                        {
                            input.Nodepath = m.Groups[2].Value;
                        } 
                        else
                        {
                            input.Nodepath = null;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Functions

        public ParameterResolverValue CallRead(HostInterface ctx, ParameterResolverValue[] args)
        {
            InputModel inp = new InputModel() { IsWriteOperation = false };
            ReturnModel ret = null;

            if (args.Length < 1) throw new ArgumentException("CallRead needs at least one argument - the address to call.");
            if (!ParseCallAddress(Convert.ToString(args[0].Value), inp)) {
                return Error.Create("Address syntax error.");
            }
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> indict) {
                    inp.Data = indict.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
                } else {
                    throw new ArgumentException("Main arguments are currently supported only as a Dictionary. Use Dict and related functions from the default library to create one.");
                }
            }
            ret = DirectCallService.Instance.Call(inp).Result;
            if (ret.IsSuccessful) {
                if (ret.BinaryData is IPostedFile pf) {
                    return new ParameterResolverValue(pf);
                } else if (ret.Data != null) {
                    if (ret.Data is Dictionary<string, object>) {

                    } else if (ret.Data is List<)
                } else {
                    return new ParameterResolverValue(null);
                }
            } else {
                return Error.Create(ret.ErrorMessage);
            }
        }

        #endregion

    }
}
