namespace Ccf.Ck.Models.Resolvers
{
    /// <summary>
    /// Describes a sinle resolver in configurations. A resolver is always implemented in a set - several resolvers in one 
    /// class. The classes are instantiated as singletons and resolver methods should behave in an immutable manner.
    /// The resolver sets implement IParameterResolversSource and should inherit from ParameterResolverSet.
    /// </summary>
    public class Resolver
    {
        /// <summary>
        /// The name under which the resolver will be available for use in expressions.
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// The original name (the same as the method name in the set implementation class
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The number of arguments for the resolver.
        /// </summary>
        public int Arguments { get; set; }
        /// <summary>
        /// Sometimes it may be better to disable a resolver insteadof removing it from the configuration.
        /// </summary>
        public bool Disable { get; set; }
        /// <summary>
        /// When true the resolver is registered under its Alias only without prefix (module) and set name (if the fullNames mode is on).
        /// This should be used only for resolvers that are part of the core standard and are used very often.
        /// </summary>
        public bool GlobalName { get; set; }
    }
}
