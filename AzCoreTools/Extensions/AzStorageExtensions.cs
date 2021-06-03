using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using AzCoreTools.Utilities.Tables;
using Azure;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using AzCoreTools.Core;

namespace AzCoreTools.Extensions
{
    public static class AzStorageExtensions
    {
        #region Common

        static readonly Func<int, bool> isLessThanZero = value => value < 0;

        private static AzStorageResponse<Pageable<T>> Query<T>(TableClient tableClient, 
            string filter,
            int? maxPerPage = null, 
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrEmptyOrWhitespace(filter);

            return AzStorageResponse<Pageable<T>>.Create(tableClient.Query<T>(
                filter,
                maxPerPage,
                cancellationToken: cancellationToken));
        }

        private static AzStorageResponse<List<T>> Query<T>(TableClient tableClient,
            Func<TableClient, int?, CancellationToken, AzStorageResponse<Pageable<T>>> func,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query<T, string, string, string>(tableClient,
                func,
                null,// funcA
                null,// funcB
                null,// funcC
                null,// paramA
                null,// paramB
                null,// paramC
                maxPerPage,
                cancellationToken,
                take);
        }

        private static AzStorageResponse<List<T>> Query<T, FTParamA>(TableClient tableClient,
            Func<TableClient, FTParamA, int?, CancellationToken, AzStorageResponse<Pageable<T>>> func,
            FTParamA param,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query<T, FTParamA, string, string>(tableClient,
                null,// func
                func,
                null,// funcB
                null,// funcC
                param,
                null,// paramB
                null,// paramC
                maxPerPage,
                cancellationToken,
                take);
        }

        private static AzStorageResponse<List<T>> Query<T, FTParamA, FTParamB>(TableClient tableClient, 
            Func<TableClient, FTParamA, FTParamB, int?, CancellationToken, AzStorageResponse<Pageable<T>>> func,
            FTParamA paramA,
            FTParamB paramB,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query<T, FTParamA, FTParamB, string>(tableClient,
                null,// func
                null,// funcA
                func,
                null,// funcC
                paramA,
                paramB,
                null,// paramC
                maxPerPage,
                cancellationToken,
                take);
        }

        private static AzStorageResponse<List<T>> Query<T, FTParamA, FTParamB, FTParamC>(TableClient tableClient,
            Func<TableClient, FTParamA, FTParamB, FTParamC, int?, CancellationToken, AzStorageResponse<Pageable<T>>> func,
            FTParamA paramA,
            FTParamB paramB,
            FTParamC paramC,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(tableClient,
                null,// func
                null,// funcA
                null,// funcC
                func,
                paramA,
                paramB,
                paramC,
                maxPerPage,
                cancellationToken,
                take);
        }

        private static AzStorageResponse<List<T>> Query<T, FTParamA, FTParamB, FTParamC>(TableClient tableClient,
            Func<TableClient, int?, CancellationToken, AzStorageResponse<Pageable<T>>> func,
            Func<TableClient, FTParamA, int?, CancellationToken, AzStorageResponse<Pageable<T>>> funcA,
            Func<TableClient, FTParamA, FTParamB, int?, CancellationToken, AzStorageResponse<Pageable<T>>> funcB,
            Func<TableClient, FTParamA, FTParamB, FTParamC, int?, CancellationToken, AzStorageResponse<Pageable<T>>> funcC,
            FTParamA paramA,
            FTParamB paramB,
            FTParamC paramC,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            ExThrower.ST_ThrowIfArgumentIsOutOfRange(take, isLessThanZero, nameof(take));
            if (func == null && funcA == null && funcB == null && funcC == null)
                ExThrower.ST_ThrowArgumentException();

            if (take == 0)
                return AzEmptyResponse<List<T>>.Create(System.Net.HttpStatusCode.BadRequest);

            AzStorageResponse<Pageable<T>> funcResponse;
            if (func != null)
                funcResponse = func(tableClient, maxPerPage, cancellationToken);
            else
            if (funcA != null)
                funcResponse = funcA(tableClient, paramA, maxPerPage, cancellationToken);
            else
            if (funcB != null)
                funcResponse = funcB(tableClient, paramA, paramB, maxPerPage, cancellationToken);
            else
                funcResponse = funcC(tableClient, paramA, paramB, paramC, maxPerPage, cancellationToken);

            if (!funcResponse.Succeeded)
                return funcResponse.InduceResponse<List<T>>();

            var result = new List<T>(take);
            var count = 0;
            foreach (var item in funcResponse.Value)
            {
                result.Add(item);

                if (++count >= take)
                    return AzStorageResponse<List<T>>.Create(result);
            }

            return AzStorageResponse<List<T>>.Create(result);
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
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient, 
                PageableQueryByPartitionKey<T>, 
                partitionKey, 
                null, 
                cancellationToken, 
                take);
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
            CancellationToken cancellationToken = default, 
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient, 
                PageableQueryByPartitionKeyStartPattern<T>, 
                startPattern, 
                null, 
                cancellationToken, 
                take);
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
            CancellationToken cancellationToken = default, 
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient, 
                PageableQueryByPartitionKeyRowKey<T>, 
                partitionKey, 
                rowKey, 
                null, 
                cancellationToken, 
                take);
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
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient, 
                PageableQueryByPartitionKeyRowKeyStartPattern<T>, 
                partitionKey, 
                rowKeyStartPattern, 
                null, 
                cancellationToken, 
                take);
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
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient,
                PageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                null,
                cancellationToken,
                take);
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
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient,
                PageableQueryByTimestamp<T>,
                timeStampFrom,
                timeStampTo,
                null,
                cancellationToken,
                take);
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
            CancellationToken cancellationToken = default,
            int take = int.MaxValue) where T : class, ITableEntity, new()
        {
            return Query(
                tableClient,
                PageableQueryByPartitionKeyTimestamp<T>,
                partitionKey,
                timeStampFrom,
                timeStampTo,
                null,
                cancellationToken,
                take);
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
            return Query(
                tableClient,
                PageableQueryAll<T>,
                null,
                cancellationToken,
                take);
        }

        #endregion
    }
}
