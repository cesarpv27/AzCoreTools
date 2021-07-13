using AzCoreTools.Core;
using Azure;
using AzCoreTools.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using CoreTools.Extensions;

namespace AzCoreTools.Helpers
{
    public class AzCoreHelper
    {
        internal static bool TryInitialize(
            Exception exception,
            IAzDetailedResponse azResponse)
        {
            return TryInitialize<Exception, IAzDetailedResponse>(exception, azResponse);
        }

        internal static bool TryInitialize<TEx, TResp>(
            TEx exception,
            TResp azResponse) where TEx : Exception where TResp : IAzDetailedResponse
        {
            if (azResponse == null)
                return false;

            azResponse.Exception = exception;
            azResponse.Message = exception.GetDepthMessages();
            azResponse.Succeeded = false;

            return true;
        }


        [Obsolete("", true)]
        internal static bool TryInitialize<T>(
            Exception exception,
            AzStorageResponse<T> azStorageResp1,
            AzStorageResponse azStorageResp2)
        {
            return TryInitialize<T, Exception, AzStorageResponse<T>, AzStorageResponse>(
                exception,
                azStorageResp1,
                azStorageResp2);
        }

        [Obsolete("", true)]
        internal static bool TryInitialize<T, TEx, TResp1, TResp2>(
            TEx exception,
            TResp1 azStorageResp1,
            TResp2 azStorageResp2 ) where TEx : Exception where TResp1 : AzStorageResponse<T> where TResp2 : AzStorageResponse
        {
            if (azStorageResp1 == null && azStorageResp2 == null)
                return false;

            if (azStorageResp1 != null)
            {
                azStorageResp1.Exception = exception;
                azStorageResp1.Message = exception.GetDepthMessages();
                azStorageResp1.Succeeded = false;
            }
            if (azStorageResp2 != null)
            {
                azStorageResp2.Exception = exception;
                azStorageResp2.Message = exception.GetDepthMessages();
                azStorageResp2.Succeeded = false;
            }

            return true;
        }
    }
}
