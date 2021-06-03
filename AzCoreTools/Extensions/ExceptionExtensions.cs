using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using Azure;
using AzCoreTools.Utilities;

namespace AzCoreTools.Extensions
{
    public static class ExceptionExtensions
    {
        #region Cosmos

        public static string GetAzErrorCodeName(this CosmosException exception)
        {
            return exception.StatusCode.ToString();
        }

        public static HttpStatusCode GetAzHttpStatusCode(this CosmosException exception)
        {
            return exception.StatusCode;
        }

        public static string GetAzErrorMessage(this CosmosException exception)
        {
            return exception.ResponseBody;// TODO: Evaluar si este texto es relevante
        }

        public static string GetAzErrorCodeName(this Exception exception)
        {
            if (exception is CosmosException _cosmosException)
                return GetAzErrorCodeName(_cosmosException);

            return string.Empty;
        }

        public static HttpStatusCode? GetAzHttpStatusCode(this Exception exception)
        {
            if (exception is CosmosException _cosmosException)
                return GetAzHttpStatusCode(_cosmosException);

            return null;
        }

        public static string GetAzErrorMessage(this Exception exception)
        {
            if (exception is CosmosException _cosmosException)
                return GetAzErrorMessage(_cosmosException);

            return string.Empty;
        }

        #endregion
    }
}
