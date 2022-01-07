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
        IEnumerable<KeyValuePair<string, HostedProc<HostInterface> >>,
        IDisposable
        where HostInterface: class
    {

        public const int DEFAULT_HARDLIMIT = 5000;
        public const string HARDLIMIT_SETTING = "AQExecutionLimit";

        protected HostInterface _Context = null;
        public HostInterface Context { 
            get { return _Context;  }
        }
        protected VariablesLibrary<HostInterface> _VariablesLibrary;

        public ActionQueryHost(HostInterface context, bool NoDefaultLibrary = false)
        {
            _Context = context;
            _VariablesLibrary = new VariablesLibrary<HostInterface>();
            if (!NoDefaultLibrary)
            {
                if (context is INodePluginContext)
                {
                    AddLibrary(_VariablesLibrary);
                    AddLibrary(DefaultLibraryNodePlugin<HostInterface>.Instance);
                }
                else if (context is IDataLoaderContext)
                {
                    AddLibrary(_VariablesLibrary);
                    AddLibrary(DefaultLibraryLoaderPlugin<HostInterface>.Instance);
                }
            }
        }

        #region IDisposable
        private bool _IsDisposed;
        public void Dispose()
        {
            if (!_IsDisposed)
            {

                _Libraries.ForEach(l => l.ClearDisposables());
                GC.SuppressFinalize(this);

                _IsDisposed = true;
            }
        }
        #endregion


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
            return ActionQueryHostBase.IsTruthyOrFalsy(v);
        }
        
        #endregion

        #region IActionQueryHostControl
        public bool StartTrace(IEnumerable<Instruction> program)
        {
            _lasttrace = null;
            if (Trace)
            {
                _stepsdone = TraceStepsLimit;
                _lasttrace = new ActionQueryTrace(program,RecordedSteps);
                return true;
            }
            return false;
        }
        private int _stepsdone = 0;
        private ActionQueryTrace _lasttrace = null;
        public bool Step(int pc, Instruction instruction, ParameterResolverValue[] arguments, IEnumerable<ParameterResolverValue> stack)
        {
            // TODO: Collect tracing data
            if (_lasttrace != null)
            {
                _lasttrace.AddStep(RecordStep(pc, instruction, arguments, stack));
            }
            if (_stepsdone <= 0) return false;
            return true;
        }

        #region Supplimentary

        public ActionQueryTrace GetTraceInfo()
        {
            return _lasttrace;
        }
        public bool Trace { get; set; }
        public int TraceStepsLimit { get; set; } = 1000;

        public int RecordedSteps { get; set; } = 10;
        public int RecordedStackDepth { get; set; } = 5;

        protected ActionQueryStep RecordStep(int pc, Instruction instruction, ParameterResolverValue[] arguments, IEnumerable<ParameterResolverValue> stack)
        {
            return new ActionQueryStep(pc, instruction, arguments, stack, _Libraries.Select(l => l.GetSymbols()));
        }

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


        #region Static tools
        public static int HardLimit(HostInterface ctx)
        {
            if (ctx is IDataLoaderContext dctx)
            {
                if (dctx.OwnContextScoped.CustomSettings.TryGetValue(HARDLIMIT_SETTING, out string val))
                {
                    if (int.TryParse(val, out int n)) return n;
                }
            }
            else if (ctx is INodePluginContext nctx)
            {
                if (nctx.OwnContextScoped.CustomSettings.TryGetValue(HARDLIMIT_SETTING, out string val))
                {
                    if (int.TryParse(val, out int n)) return n;
                }
            }
            
            return DEFAULT_HARDLIMIT; // TODO define this as a constant somewhere
        }

        public ParameterResolverValue GetVar(string varname) {
            return _VariablesLibrary.GetVar(varname);
        }

        public ParameterResolverValue SetVar(string varname, ParameterResolverValue value) {
            return _VariablesLibrary.SetVar(varname, value);
        }

        #endregion

    }
}
