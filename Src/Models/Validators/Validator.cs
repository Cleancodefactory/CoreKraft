namespace Ccf.Ck.Models.Validators
{
    /// <summary>
    /// Describes a sinle validator in configurations. A validator is always implemented in a set - several validators in one 
    /// class. The classes are instantiated as singletons and validator methods should behave in an immutable manner.
    /// The validator sets implement IParameterValidatorsSource and should inherit from ParameterCalidatorSet.
    /// </summary>
    public class Validator
    {
        /// <summary>
        /// The name under which the validator will be available for use in validation expressions.
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// The original name (the same as the method name in the set implementation class)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The number of arguments for the validator.
        /// </summary>
        public int Arguments { get; set; }
        /// <summary>
        /// Sometimes it may be better to disable a validator insteadof removing it from the configuration.
        /// </summary>
        public bool Disable { get; set; }
    }
}
