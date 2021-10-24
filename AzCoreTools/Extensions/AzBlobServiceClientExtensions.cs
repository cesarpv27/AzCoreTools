using AzCoreTools.Core;
using System;
using System.Collections.Generic;
using Azure;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.Threading;
using Azure.Storage.Blobs.Models;
using AzCoreTools.Utilities;
using AzCoreTools.Helpers;

namespace AzCoreTools.Extensions
{
    public static class AzBlobServiceClientExtensions
    {
        #region Common

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

        private static AzStorageResponse<Pageable<BlobContainerItem>> QueryBlobContainers(
            BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None,
            BlobContainerStates states = BlobContainerStates.None,
            string prefix = null,
            CancellationToken cancellationToken = default)
        {
            return AzStorageResponse<Pageable<BlobContainerItem>>.Create(blobServiceClient.GetBlobContainers(
                traits,
                states,
                prefix,
                cancellationToken), true);
        }

        private static AzStorageResponse<AsyncPageable<BlobContainerItem>> QueryBlobContainersAsync(
            BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None,
            BlobContainerStates states = BlobContainerStates.None,
            string prefix = null,
            CancellationToken cancellationToken = default)
        {
            return AzStorageResponse<AsyncPageable<BlobContainerItem>>.Create(blobServiceClient.GetBlobContainersAsync(
                traits,
                states,
                prefix,
                cancellationToken), true);
        }

        #endregion

        #region GetBlobContainers

        public static AzStorageResponse<Pageable<BlobContainerItem>> PageableGetBlobContainers(this BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None,
            BlobContainerStates states = BlobContainerStates.None,
            string prefix = null,
            CancellationToken cancellationToken = default)
        {
            return QueryBlobContainers(
                blobServiceClient,
                traits,
                states,
                prefix,
                cancellationToken);
        }

        public static AzStorageResponse<List<BlobContainerItem>> GetBlobContainers(this BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None, 
            BlobContainerStates states = BlobContainerStates.None, 
            string prefix = null, 
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake)
        {
            return TakeFromPageable(FuncHelper.Execute<BlobServiceClient, BlobContainerTraits, BlobContainerStates, string, CancellationToken, AzStorageResponse<Pageable<BlobContainerItem>>, AzStorageResponse<Pageable<BlobContainerItem>>, Pageable<BlobContainerItem>>(
                PageableGetBlobContainers,
                blobServiceClient,
                traits,
                states,
                prefix,
                cancellationToken), take);
        }

        #endregion

        #region AsyncPageable GetBlobContainers

        public static AzStorageResponse<AsyncPageable<BlobContainerItem>> AsyncPageableGetBlobContainers(
            this BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None,
            BlobContainerStates states = BlobContainerStates.None,
            string prefix = null,
            CancellationToken cancellationToken = default)
        {
            return QueryBlobContainersAsync(
                blobServiceClient,
                traits,
                states,
                prefix,
                cancellationToken);
        }

        public static async Task<AzStorageResponse<List<BlobContainerItem>>> GetBlobContainersAsync(
            this BlobServiceClient blobServiceClient,
            BlobContainerTraits traits = BlobContainerTraits.None,
            BlobContainerStates states = BlobContainerStates.None,
            string prefix = null,
            CancellationToken cancellationToken = default,
            int take = ConstProvider.DefaultTake)
        {
            return await TakeFromPageableAsync(FuncHelper.Execute<BlobServiceClient, BlobContainerTraits, BlobContainerStates, string, CancellationToken, AzStorageResponse<AsyncPageable<BlobContainerItem>>, AzStorageResponse<AsyncPageable<BlobContainerItem>>, AsyncPageable<BlobContainerItem>>(
                AsyncPageableGetBlobContainers,
                blobServiceClient,
                traits,
                states,
                prefix,
                cancellationToken), take);
        }

        #endregion
    }
}
