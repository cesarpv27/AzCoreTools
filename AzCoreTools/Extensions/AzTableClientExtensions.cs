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
            return AzExtensionTools.TakeFromPageable(response, take);
        }

        private static async Task<AzStorageResponse<List<T>>> TakeFromPageableAsync<T>(
            AzStorageResponse<AsyncPageable<T>> response,
            int take)
        {
            return await AzExtensionTools.TakeFromPageableAsync(response, take);
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

        private static AzStorageResponse<AsyncPageable<T>> QueryAsync<T>(TableClient tableClient,
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

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKey<T>(
            this TableClient tableClient,
            string partitionKey,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey).ToString(),
                maxPerPage,
                cancellationToken);
        }

        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKey' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyAsync<T>(
            this TableClient tableClient,
            string partitionKey,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKey<T>(
                tableClient,
                partitionKey,
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyAsync<T>(
            this TableClient tableClient,
            string partitionKey,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                AsyncPageableQueryByPartitionKey<T>,
                tableClient,
                partitionKey,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPattern

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKeyStartPattern<T>(this TableClient tableClient,
            string startPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, startPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, startPattern.AddLastChar())).ToString(),
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKeyStartPattern' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyStartPatternAsyncPageable<T>(this TableClient tableClient,
            string startPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKeyStartPattern<T>(
                tableClient,
                startPattern,
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyStartPatternAsync<T>(this TableClient tableClient,
            string startPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                AsyncPageableQueryByPartitionKeyStartPattern<T>,
                tableClient,
                startPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyRowKey

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKeyRowKey<T>(this TableClient tableClient,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.eq, rowKey)).ToString(),
                defaultMaxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKeyRowKey' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyRowKeyAsyncPageable<T>(this TableClient tableClient,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKeyRowKey<T>(
                tableClient,
                partitionKey,
                rowKey,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<T>> QueryByPartitionKeyRowKeyAsync<T>(this TableClient tableClient,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var innerResponse = await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, string, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                AsyncPageableQueryByPartitionKeyRowKey<T>,
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

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKeyRowKeyStartPattern<T>(this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar())))
                .ToString(),
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKeyRowKeyStartPattern' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyRowKeyStartPatternAsyncPageable<T>(
            this TableClient tableClient,
            string partitionKey,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKeyRowKeyStartPattern<T>(
                tableClient,
                partitionKey,
                rowKeyStartPattern,
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
                AsyncPageableQueryByPartitionKeyRowKeyStartPattern<T>,
                tableClient,
                partitionKey,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyStartPatternRowKeyStartPattern

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>(
            this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.ge, partitionKeyStartPattern)
                .And(TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.lt, partitionKeyStartPattern.AddLastChar()))
                .And(
                    TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.ge, rowKeyStartPattern)
                    .And(TableQueryBuilder.GenerateRowKeyFilterCondition(QueryComparison.lt, rowKeyStartPattern.AddLastChar()))).ToString(),
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKeyStartPatternRowKeyStartPattern' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyStartPatternRowKeyStartPatternAsyncPageable<T>(
            this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>(
                tableClient,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryByPartitionKeyStartPatternRowKeyStartPatternAsync<T>(
            this TableClient tableClient,
            string partitionKeyStartPattern,
            string rowKeyStartPattern,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, string, string, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                AsyncPageableQueryByPartitionKeyStartPatternRowKeyStartPattern<T>,
                tableClient,
                partitionKeyStartPattern,
                rowKeyStartPattern,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByTimestamp

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByTimestamp<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1))).ToString(),
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByTimestamp' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByTimestampAsyncPageable<T>(this TableClient tableClient,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByTimestamp<T>(
                tableClient,
                timeStampFrom,
                timeStampTo,
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
                AsyncPageableQueryByTimestamp<T>,
                tableClient,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region ByPartitionKeyTimestamp

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryByPartitionKeyTimestamp<T>(this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                TableQueryBuilder.GeneratePartitionKeyFilterCondition(QueryComparison.eq, partitionKey)
                .And(
                    TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.ge, timeStampFrom)
                    .And(TableQueryBuilder.GenerateTimestampFilterCondition(QueryComparison.lt, timeStampTo.AddMilliseconds(1)))).ToString(),
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryByPartitionKeyTimestamp' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryByPartitionKeyTimestampAsyncPageable<T>(
            this TableClient tableClient,
            string partitionKey,
            DateTime timeStampFrom,
            DateTime timeStampTo,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryByPartitionKeyTimestamp<T>(
                tableClient,
                partitionKey,
                timeStampFrom,
                timeStampTo, 
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
                AsyncPageableQueryByPartitionKeyTimestamp<T>,
                tableClient,
                partitionKey,
                timeStampFrom,
                timeStampTo,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #region QueryAll

        public static AzStorageResponse<AsyncPageable<T>> AsyncPageableQueryAll<T>(this TableClient tableClient,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return QueryAsync<T>(
                tableClient,
                string.Empty,
                maxPerPage,
                cancellationToken);
        }
        
        [Obsolete("Should use the method 'AsyncPageableQueryAll' instead. This method will be marked as error in future versions.", false)]
        public static AzStorageResponse<AsyncPageable<T>> PageableQueryAllAsyncPageable<T>(this TableClient tableClient,
            int? maxPerPage = null,
            CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            return AsyncPageableQueryAll<T>(
                tableClient,
                maxPerPage,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<T>>> QueryAllAsync<T>(this TableClient tableClient,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake) where T : class, ITableEntity, new()
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<TableClient, int?, CancellationToken, AzStorageResponse<AsyncPageable<T>>, AzStorageResponse<AsyncPageable<T>>, AsyncPageable<T>>(
                AsyncPageableQueryAll<T>,
                tableClient,
                defaultMaxPerPage, cancellationToken), take);
        }

        #endregion

        #endregion
    }
}
