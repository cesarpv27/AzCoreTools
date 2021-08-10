using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzCoreTools.Core;
using Microsoft.Azure.Cosmos;
using CoreTools.Extensions;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using AzCoreTools.Helpers;
using AzCoreTools.Utilities;

namespace AzCoreTools.Extensions
{
    public static class AzCosmosContainerExtensions
    {
        #region Common

        private static AzCosmosResponse<List<T>> TakeFromFeedIteratorAndDispose<T>(
            AzCosmosResponse<FeedIterator<T>> response,
            int take)
        {
            try
            {
                if (!TakeFromFeedIteratorAndDispose_ValidateParams(response, take))
                    return response.InduceResponse<List<T>>();

                var result = new List<T>(Math.Min(take, 1000));
                var count = 0;
                foreach (var item in response.Value.GetLazyEnumerable())
                {
                    result.Add(item);

                    if (++count >= take)
                        return AzCosmosResponse<List<T>>.Create(result, true);
                }

                return AzCosmosResponse<List<T>>.Create(result, true);
            }
            finally
            {
                if (response != null && response.Value != null)
                    response.Value.Dispose();
            }
        }

        private static AzCosmosResponse<IEnumerable<T>> LazyTakeFromFeedIteratorAndDispose<T>(
            AzCosmosResponse<FeedIterator<T>> response,
            int take)
        {
            try
            {
                if (!TakeFromFeedIteratorAndDispose_ValidateParams(response, take))
                    return response.InduceResponse<IEnumerable<T>>();

                return AzCosmosResponse<IEnumerable<T>>.Create(response.Value.GetLazyEnumerable(), true);
            }
            finally
            {
                if (response != null && response.Value != null)
                    response.Value.Dispose();
            }
        }

        private static bool TakeFromFeedIteratorAndDispose_ValidateParams<TValue>(
            AzCosmosResponse<TValue> response,
            int take)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));
            if (take <= 0)
                ExThrower.ST_ThrowArgumentException($"'{nameof(take)}' must be greater than zero");

            if (!response.Succeeded)
                return false;

            ExThrower.ST_ThrowIfArgumentIsNull(response.Value, nameof(response), "response.Value is null");

            return true;
        }

        private static AzCosmosResponse<FeedIterator<T>> Query<T>(Container container,
            string filter,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return AzCosmosResponse<FeedIterator<T>>.Create(container.GetItemQueryIterator<T>(
                filter,
                continuationToken,
                requestOptions), true);
        }
        
        private static AzCosmosResponse<FeedIterator<T>> Query<T>(Container container,
            QueryDefinition queryDefinition,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return AzCosmosResponse<FeedIterator<T>>.Create(container.GetItemQueryIterator<T>(
                queryDefinition,
                continuationToken,
                requestOptions), true);
        }

        #endregion

        #region ByFilter

        public static AzCosmosResponse<FeedIterator<T>> FeedIteratorQueryByFilter<T>(this Container container,
            string filter,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return Query<T>(
                container,
                filter,
                continuationToken,
                requestOptions);
        }

        public static AzCosmosResponse<List<T>> QueryByFilter<T>(this Container container,
            string filter,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(filter, nameof(filter), nameof(filter));
            return TakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByFilter<T>,
                container,
                filter,
                default,
                default), take);
        }

        public static AzCosmosResponse<IEnumerable<T>> LazyQueryByFilter<T>(this Container container,
            string filter,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(filter, nameof(filter), nameof(filter));
            return LazyTakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByFilter<T>,
                container,
                filter,
                default,
                default), take);
        }

        #endregion

        #region ByQueryDefinition

        public static AzCosmosResponse<FeedIterator<T>> FeedIteratorQueryByQueryDefinition<T>(this Container container,
            QueryDefinition queryDefinition,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return Query<T>(
                container,
                queryDefinition,
                continuationToken,
                requestOptions);
        }

        public static AzCosmosResponse<List<T>> QueryByQueryDefinition<T>(this Container container,
            QueryDefinition queryDefinition,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(queryDefinition, nameof(queryDefinition), nameof(queryDefinition));

            return TakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, QueryDefinition, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByQueryDefinition<T>,
                container,
                queryDefinition,
                default,
                default), take);
        }

        public static AzCosmosResponse<IEnumerable<T>> LazyQueryByQueryDefinition<T>(this Container container,
            QueryDefinition queryDefinition,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(queryDefinition, nameof(queryDefinition), nameof(queryDefinition));

            return LazyTakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, QueryDefinition, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByQueryDefinition<T>,
                container,
                queryDefinition,
                default,
                default), take);
        }

        #endregion

        #region ByPartitionKey

        public static AzCosmosResponse<FeedIterator<T>> FeedIteratorQueryByPartitionKey<T>(this Container container,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return Query<T>(
                container,
                default(string),
                continuationToken,
                requestOptions);
        }

        public static AzCosmosResponse<List<T>> QueryByPartitionKey<T>(this Container container,
            string partitionKey,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(partitionKey, nameof(partitionKey), nameof(partitionKey));
            return TakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByPartitionKey<T>,
                container,
                default,
                new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = -1
                }), take);
        }

        public static AzCosmosResponse<IEnumerable<T>> LazyQueryByPartitionKey<T>(this Container container,
            string partitionKey,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(partitionKey, nameof(partitionKey), nameof(partitionKey));
            return LazyTakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByPartitionKey<T>,
                container,
                default,
                new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = -1
                }), take);
        }

        #endregion

        #region QueryAll

        public static AzCosmosResponse<FeedIterator<T>> FeedIteratorQueryAll<T>(this Container container,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return Query<T>(
                container,
                default(string),
                continuationToken,
                requestOptions);
        }

        public static AzCosmosResponse<List<T>> QueryAll<T>(this Container container,
            int take = int.MaxValue)
        {
            return TakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryAll<T>,
                container,
                default,
                default), take);
        }
        
        public static AzCosmosResponse<IEnumerable<T>> LazyQueryAll<T>(this Container container,
            int take = int.MaxValue)
        {
            return LazyTakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryAll<T>,
                container,
                default,
                default), take);
        }

        #endregion

        #region Async

        #region Common

        private static async Task<AzCosmosResponse<List<T>>> TakeFromFeedIteratorAndDisposeAsync<T>(
            AzCosmosResponse<FeedIterator<T>> response,
            int take)
        {
            try
            {
                if (!TakeFromFeedIteratorAndDispose_ValidateParams(response, take))
                    return response.InduceResponse<List<T>>();

                var result = new List<T>(Math.Min(take, 1000));
                var count = 0;
                foreach (var item in await response.Value.GetEnumerableAsync(take))
                {
                    result.Add(item);

                    if (++count >= take)
                        return AzCosmosResponse<List<T>>.Create(result, true);
                }

                return AzCosmosResponse<List<T>>.Create(result, true);
            }
            finally
            {
                if (response != null && response.Value != null)
                    response.Value.Dispose();
            }
        }

        #endregion

        #region ByFilter

        public static async Task<AzCosmosResponse<List<T>>> QueryByFilterAsync<T>(this Container container,
            string filter,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(filter, nameof(filter), nameof(filter));
            return await TakeFromFeedIteratorAndDisposeAsync(CosmosFuncHelper.Execute<Container, string, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByFilter<T>,
                container,
                filter,
                default,
                default), take);
        }

        #endregion

        #region ByQueryDefinition

        public static async Task<AzCosmosResponse<List<T>>> QueryByQueryDefinitionAsync<T>(this Container container,
            QueryDefinition queryDefinition,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(queryDefinition, nameof(queryDefinition), nameof(queryDefinition));

            return await TakeFromFeedIteratorAndDisposeAsync(CosmosFuncHelper.Execute<Container, QueryDefinition, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByQueryDefinition<T>,
                container,
                queryDefinition,
                default,
                default), take);
        }

        #endregion

        #region ByPartitionKey

        public static async Task<AzCosmosResponse<List<T>>> QueryByPartitionKeyAsync<T>(this Container container,
            string partitionKey,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(partitionKey, nameof(partitionKey), nameof(partitionKey));
            return await TakeFromFeedIteratorAndDisposeAsync(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryByPartitionKey<T>,
                container,
                default,
                new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = -1
                }), take);
        }

        #endregion

        #region QueryAll

        public static async Task<AzCosmosResponse<List<T>>> QueryAllAsync<T>(this Container container,
            int take = int.MaxValue)
        {
            return await TakeFromFeedIteratorAndDisposeAsync(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorQueryAll<T>,
                container,
                default,
                default), take);
        }

        #endregion

        #endregion
    }
}
