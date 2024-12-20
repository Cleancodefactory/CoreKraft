using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Services;
using Ccf.Ck.Utilities.DependencyContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Execution
{
    public class TransactionScopeContext : ITransactionScopeContext
    {
        private ConcurrentDictionary<string, IPluginsSynchronizeContextScoped> _PluginsSynchronizeContext;
        private object _LockObject = new object();
        private bool _IsSuccessful = true;

        public TransactionScopeContext(IServiceCollection serviceCollection)
        {
            KraftConfigurationSettings = serviceCollection.BuildServiceProvider().GetRequiredService<KraftGlobalConfigurationSettings>();
            DependencyInjectionContainer = serviceCollection.BuildServiceProvider().GetRequiredService<DependencyInjectionContainer>();
            Logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILogger>();
            _PluginsSynchronizeContext = new ConcurrentDictionary<string, IPluginsSynchronizeContextScoped>();
            PluginServiceManager = new PluginServiceManagerImp(serviceCollection, DependencyInjectionContainer);
        }

        public IPluginServiceManager PluginServiceManager { get; }

        public ILogger Logger { get; }

        public KraftGlobalConfigurationSettings KraftConfigurationSettings { get; }

        public DependencyInjectionContainer DependencyInjectionContainer { get; }

        public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync<T>(string contextKey, ELoaderType loaderType, KraftModuleConfigurationSettings moduleConfigSettings, T plugin) where T : IPlugin
        {
            string key = $"{moduleConfigSettings.ModuleName}-{contextKey}";

            IPluginsSynchronizeContextScoped pluginsSynchronizeContextScoped = null;
            if (_PluginsSynchronizeContext.ContainsKey(key))
            {
                if (_PluginsSynchronizeContext.TryGetValue(key, out pluginsSynchronizeContextScoped))
                {
                    pluginsSynchronizeContextScoped.SetPluginName(contextKey);
                    return pluginsSynchronizeContextScoped;
                }
                Logger.LogError("_PluginsSynchronizeContext.TryGetValue(contextKey, out pluginsSynchronizeContextScoped) returned false");
            }
            else
            {
                pluginsSynchronizeContextScoped = await plugin.GetSynchronizeContextScopedAsync();
                pluginsSynchronizeContextScoped.SetPluginName(contextKey);
                pluginsSynchronizeContextScoped.CustomSettings = Utilities.GetCustomSettings(contextKey, loaderType, moduleConfigSettings);
                if (CanCache(pluginsSynchronizeContextScoped.CustomSettings))
                {
                    if (!_PluginsSynchronizeContext.TryAdd(key, pluginsSynchronizeContextScoped))
                    {
                        Logger.LogError("_PluginsSynchronizeContext.TryAdd(contextKey, pluginsSynchronizeContextScoped) returned false");
                    }
                }
            }
            return pluginsSynchronizeContextScoped;
        }

        private bool CanCache(Dictionary<string, string> customSettings)
        {
            if (customSettings != null && customSettings.ContainsKey("NoCache"))
            {
                bool result;
                if (bool.TryParse(customSettings["NoCache"].ToString(), out result))
                {
                    return !result;
                }
            }
            return true;
        }

        public void RollbackTransactions()
        {
            lock (_LockObject)
            {
                _IsSuccessful = false;  
            } 
        }

        public void CommitTransactions()
        {
            //Keep only as placeholder
        }

        public void CompleteTransactions()
        {
            if (!_IsSuccessful)
            {
                RollbackTransactionsPrivate();
            }
            else
            {
                CommitTransactionsPrivate();
            }
        }

        //public void Dispose()
        //{
        //    if (!_IsSuccessful)
        //    {
        //        RollbackTransactionsPrivate();
        //    }
        //    else
        //    {
        //        CommitTransactionsPrivate();
        //    }
        //}

        private void RollbackTransactionsPrivate()
        {
            for (int i = 0; i < _PluginsSynchronizeContext.Values.Count; i++)
            {
                ITransactionScope transactionScope = _PluginsSynchronizeContext.ElementAt(i).Value as ITransactionScope;
                if (transactionScope != null)
                {
                    transactionScope.RollbackTransaction();
                }
            }
        }

        private void CommitTransactionsPrivate()
        {
            for (int i = 0; i < _PluginsSynchronizeContext.Values.Count; i++)
            {
                ITransactionScope transactionScope = _PluginsSynchronizeContext.ElementAt(i).Value as ITransactionScope;
                if (transactionScope != null)
                {
                    transactionScope.CommitTransaction();
                }
            }
        }
    }
}