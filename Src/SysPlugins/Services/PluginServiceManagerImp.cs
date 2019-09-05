using System;
using Microsoft.Extensions.DependencyInjection;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.DependencyContainer;

namespace Ccf.Ck.SysPlugins.Services
{
    public class PluginServiceManagerImp: IPluginServiceManager
    {
        private IServiceCollection _ServiceCollection;
        private DependencyInjectionContainer _DependencyInjectionContainer;
        public PluginServiceManagerImp(IServiceCollection serviceCollection, DependencyInjectionContainer dependencyInjectionContainer)
        {
            _ServiceCollection = serviceCollection;
            _DependencyInjectionContainer = dependencyInjectionContainer;
        }

        public T GetService<T>(Type serviceType) where T : class
        {
            T t = _ServiceCollection.BuildServiceProvider().GetRequiredService(typeof(T)) as T;
            if (t != null) //the build in dependecy injection container
            {
                return t;
            }
            else //Check in the dependency injection container 
            {
                return _DependencyInjectionContainer.Get(serviceType) as T;
            }
        }
    }
}
