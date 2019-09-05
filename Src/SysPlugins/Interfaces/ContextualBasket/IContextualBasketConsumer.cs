namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// See IContextualBasket for details.
    /// This interface should be implemented by the class/service which needs to pick from the basket data items/contexts etc.
    /// How it behaves depends on its purpose and place in the architecture. Samples:
    ///     - The consumer can for example collect gradually different pieces of data as the chance to pick some of them appears. 
    ///     - A different consumer (one probably participating in some process consisting of iterations) may need to replace items
    ///         each time it is shown the basket.
    ///     ...        
    /// 
    /// </summary>
    public interface IContextualBasketConsumer
    {
        void InspectBasket(IContextualBasket basket);
    }
}
