using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using Azure;
using AzCoreTools.Utilities;

namespace AzCoreTools.Extensions
{
    public static class ExceptionExtensions
    {
        private static TOut ExecuteSwitch<TIn, TOut>(
            TIn exception,
            Func<CosmosException, TOut> cosmosExFunc,
            Func<RequestFailedException, TOut> requestFailedExFunc,
            Func<TIn, TOut> defaultFunc)
            where TIn : Exception
        {
            var exceptionType = exception.GetType();
            switch (exceptionType)
            {
                case Type _ when exceptionType == typeof(CosmosException):
                    return cosmosExFunc(exception as CosmosException);
                case Type _ when exceptionType == typeof(RequestFailedException):
                    return requestFailedExFunc(exception as RequestFailedException);
                default:
                    return defaultFunc(exception);
            }
        }

        public static string GetAzErrorCodeName(this Exception exception)
        {
            return ExecuteSwitch(exception, GetAzErrorCodeName, GetAzErrorCodeName, e => string.Empty);
        }

        public static HttpStatusCode? GetAzHttpStatusCode(this Exception exception)
        {
            return ExecuteSwitch(exception, GetAzHttpStatusCode, GetAzHttpStatusCode, e => null);
        }

        public static string GetAzErrorMessage(this Exception exception)
        {
            return ExecuteSwitch(exception, GetAzErrorMessage, GetAzErrorMessage, e => string.Empty);
        }

        #region CosmosException

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
            return exception.ResponseBody;
        }

        #endregion

        #region RequestFailedException

        public static string GetAzErrorCodeName(this RequestFailedException exception)
        {
            return exception.ErrorCode;
        }

        public static HttpStatusCode? GetAzHttpStatusCode(this RequestFailedException exception)
        {
            if (Enum.IsDefined(typeof(int), exception.Status))
                return (HttpStatusCode)exception.Status;

            return null;
        }

        public static string GetAzErrorMessage(this RequestFailedException exception)
        {
            return exception.Message;
        }

        #endregion
    }
}
