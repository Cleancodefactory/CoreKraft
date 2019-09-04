using Microsoft.Extensions.Primitives;

namespace Ccf.Ck.Utilities.MemoryCache
{
    public interface ICachingService
    {
        /// <summary>
        /// Gets the specified item key.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="itemKey">Name of the item.</param>
        /// <returns>The item</returns>
        T Get<T>(string itemKey);

        /// <summary>
        /// Inserts the specified item key.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="itemKey">Name of the item.</param>
        /// <param name="content">The content.</param>
        /// <param name="changeToken">Abstraction about cache dependencies (e.g. File- or Directory-Watchers)</param>
        void Insert<T>(string itemKey, T content, IChangeToken changeToken = null);

        /// <summary>
        /// Removes the specified item key.
        /// </summary>
        /// <param name="itemKey">Key of the item.</param>
        void Remove(string itemKey);
    }
}
