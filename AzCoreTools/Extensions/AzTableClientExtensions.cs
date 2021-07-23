using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using AzCoreTools.Utilities.Tables;
using Azure;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using AzCoreTools.Core;
using AzCoreTools.Helpers;
using AzCoreTools.Utilities;
using CoreTools.Extensions;
using AzCoreTools.Texting;
using System.Linq;
using System.Threading.Tasks;

namespace AzCoreTools.Extensions
{
    public static class AzTableClientExtensions
    {
        #region Common

        private static int? defaultMaxPerPage = null;

        private static AzStorageResponse<List<T>> TakeFromPageable<T>(
            AzStorageResponse<Pageable<T>> response,
            int take)
        {
            if (!TakeFromPageable_ValidateParams(response, take))
                return response.InduceResponse<List<T>>();

            var result = new List<T>(Math.Min(take, 1000));
            var count = 0;
            foreach (var item in response.Value)
            {
                result.Add(item);

                if (++count >= take)
                    return AzStorageResponse<List<T>>.Create(result, true);
            }

            return AzStorageResponse<List<T>>.Create(result, true);
        }

        private static async Task<AzStorageResponse<List<T>>> TakeFromPageableAsync<T>(
            AzStorageResponse<AsyncPageable<T>> response,
            int take)
        {
            if (!TakeFromPageable_ValidateParams(response, take))
                return response.InduceResponse<List<T>>();

            var result = new List<T>(Math.Min(take, 1000));
            var count = 0;

            var enumerator = response.Value.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    result.Add(enumerator.Current);

                    if (++count >= take)
                        return AzStorageResponse<List<T>>.Create(result, true);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            // TODO: In C# 8.0
            //await foreach (var item in response.Value) { ... }

            return AzStorageResponse<List<T>>.Create(result, true);
        }

        private static bool TakeFromPageable_ValidateParams<TValue>(
            AzStorageResponse<TValue> response,
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

        private static AzStorageResponse<Pageable<T>> Query<T>(TableClient tableClient, 
            string filter,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(filter);

            return AzStorageResponse<Pageable<T>>.Create(tableClient.Query<T>(
                filter,
                maxPerPage,
                cancellationToken: cancellationToken), true);
        }

        private static AzStorageResponse<AsyncPageable<T>> QueryAsyncPageable<T>(TableClient tableClient,
            string filter,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(filter);

            return AzStorageResponse<AsyncPageable<T>>.Create(tableClient.QueryAsync<T>(
                filter,
                maxPerPage,
                cancellationToken: cancellationToken), true);
        }

        #endregion

        #region ByPartitionKey

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKey<T>(this TableClient tableClient, 
            string partitionKey,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKey<T>(this TableClient tableClient, 
            string partitionKey,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKey<T>,
                tableClient,
                partitionKey,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPattern

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyStartPattern<T>(this TableClient tableClient, 
            string startPattern,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, startPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, startPattern.AddLastChar())).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyStartPattern<T>(this TableClient tableClient, 
            string startPattern,
            CancellationToken cancellationToken = default, 
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyStartPattern<T>,
                tableClient,
                startPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyRowKey

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyRowKey<T>(this TableClient tableClient, 
            string partitionKey, 
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.eq, rowKey)).ToString(),
                defaultMaxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<T> QueryByPartitionKeyRowKey<T>(this TableClient tableClient, 
            string partitionKey, 
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var innerResponse = TakeFromPageable(FuncHelper.Execute<TableClient, string, string, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyRowKey<T>,
                tableClient,
                partitionKey,
                rowKey,
                cancellationToken), ConstProvider.DefaultTake);

            if (!innerResponse.Succeeded || innerResponse.Value == null || innerResponse.Value.Count == 0)
                return innerResponse.InduceResponse<T>();

            if (innerResponse.Value.Count > 1)
            {
                var response = AzStorageResponse<T>.Create(null, false);
                response.Message = AzTextingResources.More_than_one_entity_found;
                return response;
            }

            return AzStorageResponse<T>.Create(innerResponse.Value.First(), true);
        }

        #endregion

        #region ByPartitionKeyRowKeyStartPattern

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar())))
                .ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyRowKeyStartPattern<T>,
                tableClient,
                partitionKey,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPatternRowKeyStartPattern

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, partitionKeyStartPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, partitionKeyStartPattern.AddLastChar()))
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar()))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyStartPatternRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>,
                tableClient,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByTimestamp

        public static AzStorageResponse<Pageable<T>> PageableQueryByTimestamp<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByTimestamp<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByTimestamp<T>,
                tableClient,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyTimestamp

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyTimestamp<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                    .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1)))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyTimestamp<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyTimestamp<T>,
                tableClient,
                partitionKey,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region QueryAll

        public static AzStorageResponse<Pageable<T>> PageableQueryAll<T>(this TableClient tableClient,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                string.Empty,
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryAll<T>(this TableClient tableClient,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryAll<T>,
                tableClient,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region Async

        #region ByPartitionKey

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyAsyncPageable<T>(this TableClient tableClient,
            string partitionKey,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyAsync<T>(this TableClient tableClient,
            string partitionKey,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyAsyncPageable<T>,
                tableClient,
                partitionKey,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPattern

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyStartPatternAsyncPageable<T>(this TableClient tableClient,
            string startPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, startPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, startPattern.AddLastChar())).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyStartPatternAsync<T>(this TableClient tableClient,
            string startPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyStartPatternAsyncPageable<T>,
                tableClient,
                startPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyRowKey

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyRowKeyAsyncPageable<T>(this TableClient tableClient,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.eq, rowKey)).ToString(),
                defaultMaxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<T>> QueryByPartitionKeyRowKeyAsync<T>(this TableClient tableClient,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var innerResponse = await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, string, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyRowKeyAsyncPageable<T>,
                tableClient,
                partitionKey,
                rowKey,
                cancellationToken), ConstProvider.DefaultTake);

            if (!innerResponse.Succeeded || innerResponse.Value == null || innerResponse.Value.Count == 0)
                return innerResponse.InduceResponse<T>();

            if (innerResponse.Value.Count > 1)
            {
                var response = AzStorageResponse<T>.Create(null, false);
                response.Message = AzTextingResources.More_than_one_entity_found;
                return response;
            }

            return AzStorageResponse<T>.Create(innerResponse.Value.First(), true);
        }

        #endregion

        #region ByPartitionKeyRowKeyStartPattern

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyRowKeyStartPatternAsyncPageable<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar())))
                .ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyRowKeyStartPatternAsync<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyRowKeyStartPatternAsyncPageable<T>,
                tableClient,
                partitionKey,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPatternRowKeyStartPattern

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyStartPatternRowKeyStartPatternAsyncPageable<T>(this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, partitionKeyStartPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, partitionKeyStartPattern.AddLastChar()))
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar()))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyStartPatternRowKeyStartPatternAsync<T>(this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyStartPatternRowKeyStartPatternAsyncPageable<T>,
                tableClient,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByTimestamp

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByTimestampAsyncPageable<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByTimestampAsync<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByTimestampAsyncPageable<T>,
                tableClient,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyTimestamp

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyTimestampAsyncPageable<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                    .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1)))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyTimestampAsync<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryByPartitionKeyTimestampAsyncPageable<T>,
                tableClient,
                partitionKey,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region QueryAll

        public static AzStorageResponse<AsyncPageable<T>> PageableQueryAllAsyncPageable<T>(this TableClient tableClient,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsyncPageable<T>(
                tableClient,
                string.Empty,
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryAllAsync<T>(this TableClient tableClient,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                PageableQueryAllAsyncPageable<T>,
                tableClient,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #endregion
    }
}
