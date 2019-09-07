using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IPluginAccessor<T>
    {
        T LoadPlugin(string pluginName);

        Task<IPluginsSynchronizeContextScoped> GetPluginsSynchronizeContextScoped(string contextKey, T plugin);
    }
}
