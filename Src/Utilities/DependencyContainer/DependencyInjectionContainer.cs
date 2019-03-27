using System;
using GraceInjector = Grace.DependencyInjection;
using Ccf.Ck.Utilities.DependencyContainer.Interfaces;


namespace Ccf.Ck.Utilities.DependencyContainer
{
    public class DependencyInjectionContainer : IDependencyInjectionContainer
    {
        private readonly GraceInjector.DependencyInjectionContainer _DependencyInjectionContainer;

        public DependencyInjectionContainer()
        {
            _DependencyInjectionContainer = new GraceInjector.DependencyInjectionContainer();
        }

        public void Add(Type value, Type type, object withKey)
        {
            _DependencyInjectionContainer.Configure(c => c.Export(value).AsKeyed(type, withKey));
        }

        public object Get(Type typeKey)
        {
            return _DependencyInjectionContainer.Locate(typeKey);
        }

        public object Get(Type typeKey, object withKey)
        {
            return _DependencyInjectionContainer.Locate(typeKey, withKey: withKey);
        }
    }
}
