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

namespace AzCoreTools.Extensions
{
    public static class AzTableClientExtensions
    {
        #region Common

        static readonly Func<int, bool> isLessThanZero = value => value < 0;

        private static AzStorageResponse<List<T>> TakeFromPageable<T>(
            AzStorageResponse<Pageable<T>> response,
            int take)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));
            if (take < 0)
                ExThrower.ST_ThrowArgumentException($"'{nameof(take)}' is less than cero");

            if (!response.Succeeded)
                return response.InduceResponse<List<T>>();

            ExThrower.ST_ThrowIfArgumentIsNull(response.Value, nameof(response), "response.Value is null");

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
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKey<T>,
                tableClient,
                partitionKey,
                maxPerPage, cancellationToken), take);
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
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, startPattern)).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyStartPattern<T>(this TableClient tableClient, 
            string startPattern,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default, 
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyStartPattern<T>,
                tableClient,
                startPattern,
                maxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyRowKey

        public static AzStorageResponse<Pageable<T>> PageableQueryByPartitionKeyRowKey<T>(this TableClient tableClient, 
            string partitionKey, 
            string rowKey,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return Query<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.eq, rowKey)).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyRowKey<T>(this TableClient tableClient, 
            string partitionKey, 
            string rowKey,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyRowKey<T>,
                tableClient,
                partitionKey,
                rowKey,
                maxPerPage, cancellationToken), take);
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
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern)))
                .ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyRowKeyStartPattern<T>,
                tableClient,
                partitionKey,
                rowKeyStartPattern,
                maxPerPage, cancellationToken), take);
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
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, partitionKeyStartPattern))
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyStartPatternRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>,
                tableClient,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                maxPerPage, cancellationToken), take);
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
                .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo)).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByTimestamp<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByTimestamp<T>,
                tableClient,
                timeStampFrom,
                timeStampTo,
                maxPerPage, cancellationToken), take);
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
                    .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo))).ToString(),
                maxPerPage,
                cancellationToken);
        }

        public static AzStorageResponse<List<T>> QueryByPartitionKeyTimestamp<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, string, DateTime, DateTime, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryByPartitionKeyTimestamp<T>,
                tableClient,
                partitionKey,
                timeStampFrom,
                timeStampTo,
                maxPerPage, cancellationToken), take);
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
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return TakeFromPageable(FuncHelper.Execute<TableClient, int?, CancellationToken, AzStorageResponse<Pageable<T>>, AzStorageResponse<Pageable<T>>, Pageable<T>>(
                PageableQueryAll<T>,
                tableClient,
                maxPerPage, cancellationToken), take);
        }

        #endregion
    }
}
