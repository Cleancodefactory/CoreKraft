using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.DependencyContainer;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface ITransactionScopeContext
    { 
        IPluginServiceManager PluginServiceManager { get; }

        ILogger Logger { get; }

        KraftGlobalConfigurationSettings KraftConfigurationSettings { get; }

        DependencyInjectionContainer DependencyInjectionContainer { get; }

        Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync<T>(string contextKey, ELoaderType loaderType, KraftModuleConfigurationSettings moduleConfigSettings, T plugin) where T : IPlugin;
   

        void RollbackTransactions();
        

        void CommitTransactions();
    }
}
