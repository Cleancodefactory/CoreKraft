using System;
using System.Collections.Generic;
using System.Text;
using Ccf.Ck.Libs.ActionQuery;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.Enumerations;
using System.Linq;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections;

namespace Ccf.Ck.SysPlugins.Utilities
{

    public delegate ParameterResolverValue HostedProc<H>(H arg1, ParameterResolverValue[] arg2);
    public class ActionQueryHost<HostInterface> :
        IActionQueryHost<ParameterResolverValue>, 
        IActionQueryHostControl<ParameterResolverValue>,
        IEnumerable<KeyValuePair<string, HostedProc<HostInterface> >>
        where HostInterface: class
    {

        protected HostInterface _Context = null;
        public HostInterface Context { 
            get { return _Context;  }
        }
        public ActionQueryHost(HostInterface context, bool NoDefaultLibrary = false)
        {
            _Context = context;
            if (!NoDefaultLibrary)
            {
                if (context is INodePluginContext)
                {
                    AddLibrary(new VariablesLibrary<HostInterface>());
                    AddLibrary(DefaultLibraryNodePlugin<HostInterface>.Instance);
                }
                else if (context is IDataLoaderContext)
                {
                    AddLibrary(new VariablesLibrary<HostInterface>());
                    AddLibrary(DefaultLibraryLoaderPlugin<HostInterface>.Instance);
                }
            }
        }


        #region Own callbacks
        private Dictionary<string, HostedProc<HostInterface>> _Callbacks = new Dictionary<string, HostedProc<HostInterface>>();
        public ActionQueryHost<HostInterface> Add(string name, HostedProc<HostInterface> proc)
        {
            if (proc == null)
            {
                throw new ArgumentException("proc cannot be null");
            }
            if (name == null || _Callbacks.ContainsKey(name))
            {
                throw new ArgumentException("The name of a proc cannot be null or colide with existing name.");
            }
            _Callbacks.Add(name, proc);
            return this;
        }
        protected HostedProc<HostInterface> GetProc(string name)
        {
            if (name != null && _Callbacks.ContainsKey(name))
            {
                return _Callbacks[name];
            }
            return null;
        }
        #endregion

        #region Library support
        private List<IActionQueryLibrary<HostInterface>> _Libraries = new List<IActionQueryLibrary<HostInterface>>();

        protected HostedProc<HostInterface> GetLibraryProc(string name)
        {
            for (int i= 0;i < _Libraries.Count; i++)
            {
                var p = _Libraries[i].GetProc(name);
                if (p != null) return p;
            }
            return null;
        }
        public int AddLibrary(IActionQueryLibrary<HostInterface> lib)
        {
            int index = _Libraries.IndexOf(lib);
            if (index < 0)
            {
                _Libraries.Add(lib);
                return _Libraries.Count - 1;
            }
            return index;
        }
        #endregion


        #region IActionQueryHost
        public ParameterResolverValue CallProc(string method, ParameterResolverValue[] args)
        {
            var proc = this.GetLibraryProc(method);
            if (proc == null)
            {
                proc = GetProc(method);
            }
            if (proc != null)
            {
                return proc(_Context, args);
            }
            throw new Exception($"Method {method} not found.");
        }   

        public ParameterResolverValue EvalParam(string param)
        {
            if (_Context is IDataLoaderContext dctx) {
                return dctx.Evaluate(param);
            } else if (_Context is INodePluginContext nctx) {
                return nctx.Evaluate(param);
            }else if (_Context is INodeExecutionContext ctx) { // This is not expected to be used
                return ctx.Evaluate(param);
            }

            return new ParameterResolverValue(null);

        }

        public ParameterResolverValue FromBool(bool arg)
        {
            return new ParameterResolverValue(arg);
        }

        public ParameterResolverValue FromDouble(double arg)
        {
            return new ParameterResolverValue(arg);
        }

        public ParameterResolverValue FromInt(int arg)
        {
            return new ParameterResolverValue(arg);
        }

        public ParameterResolverValue FromNull()
        {
            return new ParameterResolverValue(null);
        }

        public ParameterResolverValue FromString(string arg)
        {
            return new ParameterResolverValue(arg);
        }

        public bool IsTruthyOrFalsy(ParameterResolverValue v)
        {
            if (v.ValueType == EResolverValueType.ValueType || v.ValueType == EResolverValueType.ContentType)
            {
                // TODO: Redo this with converter to cover all types. Currently other types are unlikely to happen.
                if (v.Value == null) return false;
                if (v.Value is int i) return i != 0;
                if (v.Value is uint ui) return ui != 0;
                if (v.Value is double d) return d != 0;
                if (v.Value is long l) return l != 0;
                if (v.Value is ulong ul) return ul != 0;
                if (v.Value is short sh) return sh != 0;
                if (v.Value is ushort ush) return ush != 0;
                if (v.Value is char ch) return ch != 0;
                if (v.Value is byte bt) return bt != 0;
                if (v.Value is bool b) return b;
                if (v.Value is string s) return !string.IsNullOrWhiteSpace(s);
            } 
            else if (v.ValueType == EResolverValueType.Invalid || v.ValueType == EResolverValueType.Skip)
            {
                return false;
            } 
            return false;
        }
        #endregion

        #region IActionQueryHostControl
        public bool StartTrace()
        {
            if (Trace)
            {
                _stepsdone = TraceStepsLimit;
                return true;
            }
            return false;
        }
        private int _stepsdone = 0;
        public bool Step(int pc, Instruction instruction, ParameterResolverValue[] arguments, IEnumerable<ParameterResolverValue> stack = null)
        {
            // TODO: Collect tracing data
            if (_stepsdone <= 0) return false;
            return true;
        }

        #region Supplimentary
        public bool Trace { get; set; }
        public int TraceStepsLimit { get; set; } = 1000;
        #endregion
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<string, HostedProc<HostInterface>>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, HostedProc<HostInterface>>>)_Callbacks).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_Callbacks).GetEnumerator();
        }
        #endregion
    }
}
