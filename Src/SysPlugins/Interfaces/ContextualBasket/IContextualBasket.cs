using System;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// This interface enables scoped services to pickup references to actual contextual data. Unlike
    /// similar techniques used mostly during initialization this one is not bound to any specific lifecycle moment.
    /// It is up to the developer to decide what and where to do and here is a relevant sample scenario:
    /// 
    /// A class/context with lifecycle that includes a smaller lifecycle that repeats many times, but with slightly different work 
    /// data and parameters determined from the outside. Then for each iteration of the internal cycle the instance that implements the
    /// ICeontextualBasket
    /// 
    /// </summary>
    public interface IContextualBasket
    {
        /// <summary>
        /// Asks the basket if an item of the given type exists in the basket.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool HasBasketItem(Type t);
        object PickBasketItem(Type t);
    }
    
}
