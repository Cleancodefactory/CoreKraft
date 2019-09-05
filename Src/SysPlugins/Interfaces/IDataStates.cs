namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// A helper catalog for the values of the data state.
    /// Enables any code that works with the pipeline data to compare states without referencing constants from an additional assembly and
    /// makes possible these constants to be dynamic and different in different pipelines or even nodes (not necessary at this moment, but having the
    /// option in place is a plus).
    /// </summary>
    public interface IDataStates
    {
        #region State values
        /// <summary>
        /// The value for unchanged state (usually returned by a property or a state manager)
        /// </summary>
        string StateUnchanged { get; }
        /// <summary>
        /// The value for new state (usually returned by a property or a state manager)
        /// </summary>
        string StateNew { get; }
        /// <summary>
        /// The value for updated state (usually returned by a property or a state manager)
        /// </summary>
        string StateUpdated { get; }
        /// <summary>
        /// The value for deleted (for deletion) state (usually returned by a property or a state manager)
        /// </summary>
        string StateDeleted { get; }
        #endregion

        #region State property
        /// <summary>
        /// In property based state maring this returns the name of the property/dictionary element that holds the state
        /// </summary>
        string StatePropertyName { get; }
        #endregion

    }
}
