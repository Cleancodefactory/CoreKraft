using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Utilities.GlobalAccessor
{
    public class GlobalAccessor
    {
        private IServiceProvider _ServiceProvider;
        private SystemPersister _SystemPersister;
        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        private static readonly Lazy<GlobalAccessor> _Instance = new(() => new GlobalAccessor());
        public static GlobalAccessor Instance => _Instance.Value;


        public void Initialize(IServiceProvider serviceProvider)
        {
            if (_ServiceProvider == null)
            {
                _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Service provider cannot be null.");
                _KraftGlobalConfigurationSettings = serviceProvider.GetService(typeof(KraftGlobalConfigurationSettings)) as KraftGlobalConfigurationSettings;
                _SystemPersister = new SystemPersister(_KraftGlobalConfigurationSettings);
            }
            else
            {
                //During restart the service provider may be re-initialized, so we can skip this check.
                //throw new InvalidOperationException("GlobalAccessor has already been initialized. It can only be initialized once.");
            }
        }

        public void AddOperation<T>(T operation) where T : IOperation
        {
            if (_SystemPersister == null)
            {
                throw new InvalidOperationException("GlobalAccessor has not been initialized. Call Initialize first.");
            }
            _SystemPersister.StoreOperation(operation);
        }

        public IEnumerable<T> GetOperations<T>() where T : IOperation
        {
            if (_SystemPersister == null)
            {
                throw new InvalidOperationException("GlobalAccessor has not been initialized. Call Initialize first.");
            }

            return _SystemPersister.GetOperations<T>();
        }

        public void RemoveOperation<T>(T operation) where T : IOperation
        {
            if (_SystemPersister == null)
            {
                throw new InvalidOperationException("GlobalAccessor has not been initialized. Call Initialize first.");
            }
            _SystemPersister.RemoveOperation<T>(operation);
        }

        internal KraftGlobalConfigurationSettings GetKraftGlobalConfigurationSettings()
        {
            if (_KraftGlobalConfigurationSettings == null)
            {
                throw new InvalidOperationException("GlobalAccessor has not been initialized. Call Initialize first.");
            }
            return _KraftGlobalConfigurationSettings;
        }
    }
}
