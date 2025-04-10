using System.Linq;
using System.Collections.Generic;
using Ccf.Ck.SysPlugins.Interfaces;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Utilities
{
    /// <summary>
    /// Imutable, singleton class
    /// </summary>
    public class DataStateUtility : IDataStateHelper<IDictionary<string, object>>
    {
        #region Construction and singleton
        private DataStateUtility() { }

        public static DataStateUtility Instance
        {
            get
            {
                return SingletonHolder.instance;
            }
        }

        protected class SingletonHolder
        {
            static SingletonHolder() { }
            internal static readonly DataStateUtility instance = new DataStateUtility();
        }
        #endregion


        #region IDataStates and IDataStatesHelper
        public string StateUnchanged { get => STATE_PROPERTY_UNCHANGED; }
        public string StateNew { get => STATE_PROPERTY_INSERT; }
        public string StateUpdated { get => STATE_PROPERTY_UPDATE; }
        public string StateDeleted { get => STATE_PROPERTY_DELETE; }
        public string StatePropertyName { get => STATE_PROPERTY_NAME; }

        public string GetDataState(IDictionary<string, object> element)
        {
            if (element != null && element.ContainsKey(StatePropertyName))
            {
                return MakeValidState(element[StatePropertyName] as string);
            }
            return null;
        }
        /// <summary>
        /// Invalid states will cause no changes silently by default. Set WRONG_STATE_EXCEPTIONS to the build to throw exceptions instead.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="state"></param>
        public void SetDataState(IDictionary<string, object> element, string state)
        {
            if (_STATE_PROPERTY_DONOT_CHANGE)
            {
                if (element.ContainsKey(StatePropertyName))
                {
                    element.Remove(StatePropertyName);
                }
                return;
            }
            if (element != null)
            {
                var _state = MakeValidState(state);
                if (_state != null)
                {
                    element[StatePropertyName] = _state;
                }
                else if (state == null)
                {
                    element.Remove(StatePropertyName);
                }
                else
                {
                    // wrong one - do nothing
#if WRONG_STATE_EXCEPTIONS
                    throw new ArgumentException("The state value is incorrect.");
#endif
                }
            }
        }
        public bool IsDataStateOf(IDictionary<string, object> element, string state)
        {
            if (state is string s && element != null && element.ContainsKey(StatePropertyName))
            {
                return (string.Compare(s, state, false) == 0);
            }
            return false;
        }

        public string GetDataState(object element)
        {
            return GetDataState(element as IDictionary<string, object>);
        }

        public void SetDataState(object element, string state)
        {
            SetDataState(element as IDictionary<string, object>, state);
        }
        public void SetUnchanged(object v) { SetDataState(v, StateUnchanged); }
        public void SetUpdated(object v) { SetDataState(v, StateUpdated); }
        public void SetNew(object v) { SetDataState(v, StateNew); }
        public void SetDeleted(object v) { SetDataState(v, StateDeleted); }

        public bool IsDataStateOf(object element, string state)
        {
            return IsDataStateOf(element as IDictionary<string, object>, state);
        }
        #endregion

        #region Non-interface local helpers
        protected string MakeValidState(string state)
        {
            if (STATE_VALID_VALUES.Any(s => s == state)) return state;
            return null;
        }
        #endregion
    }
}
