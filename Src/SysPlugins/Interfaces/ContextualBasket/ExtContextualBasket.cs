namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Extension methods for contextual baskets
    /// </summary>
    public static class ExtContextualBasket
    {
        public static T PickBasketItem<T>(this IContextualBasket basket) where T: class {
            if (basket == null) return default(T);
            return basket.PickBasketItem(typeof(T)) as T;
        }
    }
}
