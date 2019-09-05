using System;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IPluginServiceManager
    {
        /// <summary>
        /// The System.Plugins will be able to access registered types in App.Startup
        /// </summary>
        /// <typeparam name="T">the registered implementation type</typeparam>
        /// <param name="serviceType"></param>
        /// <returns>The injected type either from the build in .NET core dependency injection (DI) container or from Grace DI.</returns>
        T GetService<T>(Type serviceType) where T : class;
    }
}
