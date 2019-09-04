using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IPlugin
    {
        Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync();
    }
}
