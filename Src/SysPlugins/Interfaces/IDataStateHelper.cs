namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Works together with IDataStates to provide a helping hand to code processing pipeline data.
    /// The implementations are not typically universal, but based on a common format determined by the entire pipline and not specific 
    /// (potentially different for different parts, nodes).
    /// </summary>
    public interface IDataStateHelper: IDataStates
    {
        /// <summary>
        /// MUST return one of the state values defined by IDataStates interface or null if there is no state.
        /// Code counting on this interface MUST ignore completely any workload that has a null state - ignore
        /// the data element and any dependent data elements.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        string GetDataState(object element);
        /// <summary>
        /// Sets the data state of the element.
        /// A null for the state SHOULD clear the state (no state if applicable, null or unchanged if the specifics do not allow other approach)
        /// An incorrect but non-null value for the state SHOULD cause excepttion or cause no changes if exception is against the architecture's policy
        /// (the behavior should be well-documented). Being a "should" rule, this can be implemented differently, but is strongly discoraged.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="state"></param>
        void SetDataState(object element, string state);

        /// <summary>
        /// Sets the data state of v to unchanged
        /// </summary>
        /// <param name="v"></param>
        void SetUnchanged(object v);
        /// <summary>
        /// Sets the data state of v to updated
        /// </summary>
        /// <param name="v"></param>
        void SetUpdated(object v);
        /// <summary>
        /// Sets the data state of v to new
        /// </summary>
        /// <param name="v"></param>
        void SetNew(object v);
        /// <summary>
        /// Sets the data state of v to deleted
        /// </summary>
        /// <param name="v"></param>
        void SetDeleted(object v);
        /// <summary>
        /// Checks if the state of the element is the given one
        /// </summary>
        /// <param name="element"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        bool IsDataStateOf(object element, string state);
    }

    public interface IDataStateHelper<ETYPE>: IDataStateHelper
    {
        string GetDataState(ETYPE element);
        void SetDataState(ETYPE element, string state);
        bool IsDataStateOf(ETYPE element, string state);
    }
}
