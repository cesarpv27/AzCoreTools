using AzCoreTools.Core;
using AzCoreTools.Texting;
using CoreTools.Extensions;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Extensions
{
    public static class AzCosmosFeedIteratorExtensions
    {
        #region Common

        private static async Task<KeyValuePair<FeedResponse<T>, List<T>>> GetFeedsAsync<T>(
            this FeedIterator<T> feedIterator,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (take <= 0)
                ExThrower.ST_ThrowArgumentOutOfRangeException(nameof(take), 
                    AzTextingResources.Param_must_be_grather_than_zero(nameof(take)));

            var items = new List<T>(Math.Min(take, 1000));

            var count = 0;
            FeedResponse<T> _feedResponse = null;
            while (feedIterator.HasMoreResults)
            {
                _feedResponse = await feedIterator.ReadNextAsync(cancellationToken);
                foreach (T _item in _feedResponse)
                {
                    items.Add(_item);

                    if (++count >= take)
                        return new KeyValuePair<FeedResponse<T>, List<T>>(_feedResponse, items);
                }
            }

            return new KeyValuePair<FeedResponse<T>, List<T>>(_feedResponse, items);
        }

        #endregion

        #region Get enumerable

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
            FeedResponse<T> _feedResponse;
            while (feedIterator.HasMoreResults)
            {
                _feedResponse = feedIterator.ReadNextAsync(cancellationToken).WaitAndUnwrapException();
                foreach (T _item in _feedResponse)
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
            return feedIterator.GetEnumerableAsync(take, cancellationToken).WaitAndUnwrapException();
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
            return (await feedIterator.GetFeedsAsync(take, cancellationToken)).Value;
        }

        #endregion

        #region Get response

        /// <summary>
        /// Get the items from the cosmos service.
        /// </summary>
        /// <typeparam name="T">Entity model type.</typeparam>
        /// <param name="feedIterator">An iterator to go through the items.</param>
        /// <param name="take">Amount of items to take.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>The <see cref="AzCosmosResponse{List{T}}"/> indicating the result of the operation,
        /// containing a collection of entity models obtained and serialized from cosmos service.</returns>
        public static AzCosmosResponse<List<T>> GetResponse<T>(
            this FeedIterator<T> feedIterator,
            int take,
            CancellationToken cancellationToken = default)
        {
            return feedIterator.GetResponseAsync(take, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Get the items from the cosmos service.
        /// </summary>
        /// <typeparam name="T">Entity model type.</typeparam>
        /// <param name="feedIterator">An iterator to go through the items.</param>
        /// <param name="take">Amount of items to take.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>The <see cref="AzCosmosResponse{List{T}}"/> indicating the result of the operation,
        /// containing a collection of entity models obtained and serialized from cosmos service, 
        /// that was created contained within a System.Threading.Tasks.Task object representing 
        /// the service response for the asynchronous operation.</returns>
        public static async Task<AzCosmosResponse<List<T>>> GetResponseAsync<T>(
            this FeedIterator<T> feedIterator,
            int take,
            CancellationToken cancellationToken = default)
        {
            var result = await feedIterator.GetFeedsAsync(take, cancellationToken);

            return AzCosmosResponse<List<T>>.Create(result.Value, true, default, result.Key.ContinuationToken);
        }

        #endregion
    }
}
