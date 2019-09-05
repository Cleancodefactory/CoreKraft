using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Libs.ResolverExpression;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.Interfaces
{
    /// <summary>
    /// This interfface should be implemented by plugins or/and internal resolver sources - a class exposing one or more methods as resolvers.
    /// Implementation has to be singleton oriented.
    /// </summary>
    public interface IParameterResolversSource
    {
        ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver(string name);
    }
}
