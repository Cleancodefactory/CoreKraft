using Ccf.Ck.Models.Resolvers;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IParametersContext
    {
        ParameterResolverValue this[string param] { get; }
    }
}