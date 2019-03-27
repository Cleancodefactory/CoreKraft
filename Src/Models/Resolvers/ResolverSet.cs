using System.Collections.Generic;

namespace Ccf.Ck.Models.Resolvers
{
    /// <summary>
    /// This one is not finished. Describes a class in an assembly and subset of the resolvers implemented in it.
    /// </summary>
    public class ResolverSet
    {
        // public string className { get; set; } // We will these later

        /// <summary>
        /// The name of the resolver set. It must be specified explicitly to avoid collisions. Using the class name can cause false sense
        /// of almost guaranteed uniqueness, but using the full names will make expressions hard to read and using only the class name (without namespacing)
        /// makes it more likely to have collisions.
        /// Proposed reserved namespaces (prefixes): _Sys, _BuiltIn, _bk/_BK
        /// We should avoid these for now as module names.
        /// </summary>
        public string Name{ get; set; }
        /// <summary>
        /// Register all the resolvers with global names (no dots)
        /// </summary>
        public bool GlobalNames { get; set; }
        /// <summary>
        /// List of the methods exposed as resolvers
        /// </summary>
        public List<Resolver> Resolvers { get; set; }
    }
}
