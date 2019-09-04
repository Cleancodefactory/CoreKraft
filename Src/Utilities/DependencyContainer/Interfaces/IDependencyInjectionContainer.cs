using System;

namespace Ccf.Ck.Utilities.DependencyContainer.Interfaces
{
    public interface IDependencyInjectionContainer
    {
        void Add(Type value, Type type, object withKey);

        object Get(Type typeKey, object withKey);
    }
}
