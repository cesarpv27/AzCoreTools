using CoreTools.Extensions;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzCoreTools.Extensions
{
    public static class AzCosmosFeedIteratorExtensions
    {
        /// <summary>
        /// Get the items from the cosmos service by lazy evaluation
        /// </summary>
        /// <typeparam name="T">Entity model type</typeparam>
        /// <param name="feedIterator">An iterator to go through the items</param>
        /// <returns>A collection of entity models serialized as type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> GetLazyEnumerable<T>(this FeedIterator<T> feedIterator)
        {
            while (feedIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = feedIterator.ReadNextAsync().WaitAndUnwrapException();
                foreach (T _item in currentResultSet)
                    yield return _item;
            }
        }

        /// <summary>
        /// Get the items from the cosmos service by eager evaluation
        /// </summary>
        /// <typeparam name="T">Entity model type</typeparam>
        /// <param name="feedIterator">An iterator to go through the items</param>
        /// <param name="take">Amount of items to take</param>
        /// <returns>A collection of entity models serialized as type <typeparamref name="T"/>.</returns>
        public static async Task<IEnumerable<T>> GetEnumerableAsync<T>(this FeedIterator<T> feedIterator, int take)
        {
            var items = new List<T>();

            var count = 0;
            while (feedIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await feedIterator.ReadNextAsync();
                foreach (T _item in currentResultSet)
                {
                    items.Add(_item);

                    if (++count >= take)
                        return items;
                }
            }

            return items;
        }
    }
}
