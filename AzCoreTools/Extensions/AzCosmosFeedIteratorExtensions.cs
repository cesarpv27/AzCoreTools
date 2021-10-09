using CoreTools.Extensions;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzCoreTools.Extensions
{
    public static class AzCosmosFeedIteratorExtensions
    {
        /// <summary>
        /// Get the items from the cosmos service using deferred execution.
        /// </summary>
        /// <typeparam name="T">Entity model type.</typeparam>
        /// <param name="feedIterator">An iterator to go through the items.</param>
        /// <param name="take">Amount of items to take.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>A collection of entity models serialized as type <see cref="List{T}"/> indicating the result of the operation.</returns>
        public static IEnumerable<T> GetLazyEnumerable<T>(
            this FeedIterator<T> feedIterator,
            int take,
            CancellationToken cancellationToken = default)
        {
            var count = 0;
            FeedResponse<T> currentResultSet;
            while (feedIterator.HasMoreResults)
            {
                currentResultSet = feedIterator.ReadNextAsync(cancellationToken).WaitAndUnwrapException();
                foreach (T _item in currentResultSet)
                {
                    if (++count > take)
                        yield break;

                    yield return _item;
                }
            }
        }

        /// <summary>
        /// Get the items from the cosmos service.
        /// </summary>
        /// <typeparam name="T">Entity model type.</typeparam>
        /// <param name="feedIterator">An iterator to go through the items.</param>
        /// <param name="take">Amount of items to take.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>A collection of entity models serialized as type <see cref="List{T}"/> indicating the result of the operation.</returns>
        public static List<T> GetEnumerable<T>(
            this FeedIterator<T> feedIterator, 
            int take,
            CancellationToken cancellationToken = default)
        {
            return GetEnumerableAsync(feedIterator, take, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Get the items from the cosmos service.
        /// </summary>
        /// <typeparam name="T">Entity model type.</typeparam>
        /// <param name="feedIterator">An iterator to go through the items.</param>
        /// <param name="take">Amount of items to take.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>A collection of entity models serialized as type <see cref="List{T}"/> indicating the result of the operation, 
        /// that was created contained within a System.Threading.Tasks.Task object representing 
        /// the service response for the asynchronous operation.</returns>
        public static async Task<List<T>> GetEnumerableAsync<T>(
            this FeedIterator<T> feedIterator, 
            int take,
            CancellationToken cancellationToken = default)
        {
            var items = new List<T>(take);

            var count = 0; 
            FeedResponse<T> currentResultSet;
            while (feedIterator.HasMoreResults)
            {
                currentResultSet = await feedIterator.ReadNextAsync(cancellationToken);
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
