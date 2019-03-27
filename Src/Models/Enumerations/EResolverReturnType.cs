namespace Ccf.Ck.Models.Enumerations
{
    /// <summary>
    /// Describes the purpose of the value:
    /// - a value to be bound to command (ValueType);
    /// - a literal to be replaced in the command text (ContentType);
    /// - invalid value, result of an error (especially in modes with supressed exceptions), impossible calculation etc. (Invalid);
    /// - Nonstorable value, usually a reference to somethnig that another component is supposed to use (another resolver or a plugin that should
    ///     use it as a reference to something it needs). These values MUST not be stored in database or other storages and should be skept or treated as nulls (Nonstorable)
    /// </summary>
    public enum EResolverValueType
    {
        /// <summary>
        /// The parameter value is just a value
        /// </summary>
        ValueType = 0,
        /// <summary>
        /// The parameter value is a literal content e.g. SQL fragment, some pseudo-language snipet ...
        /// </summary>
        ContentType = 1,
        /// <summary>
        /// Value is either a user defined sql variable, either anything else that math the regex, but is intended to remain unchanged.
        /// </summary>
        Skip = 2,
        /// <summary>
        /// When an invalid value is returned this should be set
        /// </summary>
        Invalid = -1,
        /// <summary>
        /// Value intended for use by another resolver or a component which contains data that cannot or should not be stored by DataLoaders and/or custom plugins.
        /// </summary>
        Nonstorable = -2,

    }
}
