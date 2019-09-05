using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Execution
{
    public class PluginAccessorImp<T> : IPluginAccessor<T> where T : class, IPlugin
    {
        private ITransactionScopeContext _TransactionScope;
        private KraftModuleConfigurationSettings _KraftModuleConfigurationSettings;

        public PluginAccessorImp(ITransactionScopeContext transactionScope, KraftModuleConfigurationSettings moduleSettings)
        {
            _TransactionScope = transactionScope;
            _KraftModuleConfigurationSettings = moduleSettings;
        }

        public async Task<IPluginsSynchronizeContextScoped> GetPluginsSynchronizeContextScoped(string contextKey, T plugin)
        {
            return await _TransactionScope.GetSynchronizeContextScopedAsync(contextKey, GetLoaderType(), _KraftModuleConfigurationSettings, plugin);
        }

        public T LoadPlugin(string pluginName)
        {
            return Utilities.GetPlugin<T>(pluginName, _TransactionScope.DependencyInjectionContainer, _KraftModuleConfigurationSettings, GetLoaderType());
        }

        private ELoaderType GetLoaderType()
        {
            if (this is IPluginAccessor<INodePlugin>)
            {
                return ELoaderType.CustomPlugin;
            }
            if (this is IPluginAccessor<IDataLoaderPlugin>)
            {
                return ELoaderType.DataLoader;
            }
            return ELoaderType.None;
        }
    }
}
