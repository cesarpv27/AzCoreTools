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

        private static int? defaultMaxPerPage = null;

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
            ExThrower.ST_ThrowIfArgumentIsNull(filter);

            return AzCosmosResponse<FeedIterator<T>>.Create(container.GetItemQueryIterator<T>(
                filter,
                continuationToken,
                requestOptions), true);
        }

        #endregion

        #region ByPartitionKey

        public static AzCosmosResponse<FeedIterator<T>> FeedIteratorByPartitionKey<T>(this Container container,
            string continuationToken = null,
            QueryRequestOptions requestOptions = null)
        {
            return Query<T>(
                container,
                null,
                continuationToken,
                requestOptions);
        }

        public static AzCosmosResponse<List<T>> ByPartitionKey<T>(this Container container,
            string partitionKey,
            int take = ConstProvider.DefaultTake)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(partitionKey, nameof(partitionKey), nameof(partitionKey));
            return TakeFromFeedIteratorAndDispose(CosmosFuncHelper.Execute<Container, string, QueryRequestOptions, AzCosmosResponse<FeedIterator<T>>, AzCosmosResponse<FeedIterator<T>>, FeedIterator<T>>(
                FeedIteratorByPartitionKey<T>,
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
                null,
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

        #endregion
    }
}
