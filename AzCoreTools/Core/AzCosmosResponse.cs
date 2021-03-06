using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
using AzCoreTools.Core.Validators;
using AzCoreTools.Core.Interfaces;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using AzCoreTools.Helpers;
using System.Net;
using AzCoreTools.Texting;

namespace AzCoreTools.Core
{
    public class AzCosmosResponse<T> : Response<T>, IAzCosmosResponse<T>
    {
        protected internal Response<T> _response;
        protected T _value;

        public virtual bool Succeeded { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual string Message { get; set; }
        public virtual string ContinuationToken { get; set; }

        /// <summary>
        /// The content of the response
        /// </summary>
        public virtual T Value
        {
            get
            {
                return _value;
            }
        }

        #region Initializers

        private void InitializeWithoutValidations<RTIn>(
            RTIn response, 
            T value, 
            string continuationToken = default) 
            where RTIn : Response<T>
        {
            _response = response;
            _value = value;
            ContinuationToken = continuationToken;

            if (_response != null)
                Succeeded = ResponseValidator.CosmosResponseSucceeded<Response<T>, T>(_response);
        }

        protected virtual void Initialize<RTIn>(
            RTIn response, 
            T value, 
            string continuationToken = default) 
            where RTIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response, value, continuationToken);
        }

        protected virtual void Initialize<RTIn>(RTIn response) where RTIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            Initialize(response, response.Resource);
        }

        protected virtual void Initialize(T value, string continuationToken = default)
        {
            InitializeWithoutValidations<Response<T>>(default, value, continuationToken);
        }

        protected virtual void InitializeWithException<TEx>(
            TEx exception,
            string messagePrefix = AzTextingResources.Exception_message) 
            where TEx : Exception
        {
            AzCoreHelper.TryInitialize(exception, this, messagePrefix);
        }

        protected virtual void Initialize<GenT, RTSource>(RTSource source, T value = default) 
            where RTSource : AzCosmosResponse<GenT>, new()
        {
            InitializeWithoutValidations<Response<T>>(default, value);
            Succeeded = source.Succeeded;
            Message = source.Message;
            Exception = source.Exception;
        }

        protected virtual void Initialize(TransactionalBatchResponse response, T value)
        {
            InitializeWithoutValidations<Response<T>>(default, value);
            Succeeded = response.IsSuccessStatusCode;
            Message = response.ErrorMessage;
        }

        #endregion

        #region Creators

        public static TOut CreateNew<TOut>() where TOut : AzCosmosResponse<T>, new()
        {
            return new TOut();
        }
        
        public static TOut CreateNew<GenT, TOut>() where TOut : AzCosmosResponse<GenT>, new()
        {
            return new TOut();
        }

        public static AzCosmosResponse<T> Create<RTIn>(RTIn response) 
            where RTIn : Response<T>
        {
            return Create<RTIn, AzCosmosResponse<T>>(response);
        }

        public static TOut Create<RTIn, TOut>(RTIn response) 
            where RTIn : Response<T> 
            where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

            return result;
        }
        
        public static TOut Create<RTIn, TOut>(
            RTIn response, 
            T value, 
            Func<RTIn, string> getContinuationToken) 
            where RTIn : Response<T> 
            where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response, value, getContinuationToken(response));

            return result;
        }
        
        public static AzCosmosResponse<T> Create<RTIn>(
            RTIn response, 
            T value, 
            Func<RTIn, string> getContinuationToken) 
            where RTIn : Response<T> 
        {
            var result = CreateNew<AzCosmosResponse<T>>();
            result.Initialize(response, value, getContinuationToken(response));

            return result;
        }
        
        public static TOut CreateFromTransactionalBatchResponse<TBResp, TOut>(TBResp response) 
            where TBResp : TransactionalBatchResponse
            where TOut : AzCosmosResponse<TransactionalBatchResponse>, new()
        {
            var result = CreateNew<TransactionalBatchResponse, TOut>();
            result.Initialize(response, response);

            return result;
        }

        public static AzCosmosResponse<T> Create(
            T value,
            bool succeeded,
            string message = default,
            string continuationToken = default)
        {
            return Create<AzCosmosResponse<T>>(value, succeeded, message, continuationToken);
        }

        public static TOut Create<TOut>(
            T value,
            bool succeeded,
            string message = default, 
            string continuationToken = default) 
            where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(value, continuationToken);

            result.Succeeded = succeeded;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// Creates an unsuccessful response of type <see cref="AzCosmosResponse{T}"/>.
        /// </summary>
        /// <param name="message">Message of resulting <see cref="AzCosmosResponse{T}"/>.</param>
        /// <returns>The <see cref="AzCosmosResponse{T}"/> indicating the result of the operation.</returns>
        public static AzCosmosResponse<T> Create(string message)
        {
            return Create<AzCosmosResponse<T>>(message);
        }

        /// <summary>
        /// Creates an unsuccessful response of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TOut">A custom model of type <see cref="AzCosmosResponse{T}"/>.</typeparam>
        /// <param name="message">Message of resulting <typeparamref name="TOut"/>.</param>
        /// <returns>The <typeparamref name="TOut"/> indicating the result of the operation.</returns>
        public static TOut Create<TOut>(string message) 
            where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();

            result.InitializeWithoutValidations<Response<T>>(default, default);
            result.Succeeded = false;
            result.Message = message;

            return result;
        }

        public static TOut Create<TOut>(
            Exception exception,
            string messagePrefix = AzTextingResources.Exception_message) 
            where TOut : AzCosmosResponse<T>, new()
        {
            return CreateWithException<Exception, TOut>(exception, messagePrefix);
        }

        public static TOut CreateWithException<TEx, TOut>(
            TEx exception,
            string messagePrefix = AzTextingResources.Exception_message) 
            where TEx : Exception 
            where TOut : AzCosmosResponse<T>, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(exception, nameof(exception));

            var result = CreateNew<TOut>();
            result.InitializeWithException(exception, messagePrefix);

            return result;
        }

        #endregion

        #region Abstract class implementation

        /// <summary>
        /// The content of the response
        /// </summary>
        public override T Resource
        {
            get
            {
                return Value;
            }
        }

        public override Headers Headers
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.Headers;
            }
        }

        public override HttpStatusCode StatusCode
        {
            get
            {
                if (_response != null)
                    return _response.StatusCode;
                if (Succeeded)
                    return HttpStatusCode.OK;

                return HttpStatusCode.Conflict;
            }
        }

        public override double RequestCharge
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.RequestCharge;
            }
        }

        public override string ActivityId
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.ActivityId;
            }
        }

        public override string ETag
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.ETag;
            }
        }

        public override CosmosDiagnostics Diagnostics
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.Diagnostics;
            }
        }

        #endregion

        public virtual AzCosmosResponse<GenT> InduceResponse<GenT>(GenT value = default)
        {
            return InduceResponse<GenT, AzCosmosResponse<GenT>>(value);
        }

        public virtual TOut InduceResponse<GenT, TOut>(GenT value = default)
            where TOut : AzCosmosResponse<GenT>, new()
        {
            var result = CreateNew<GenT, TOut>();

            result.Initialize<T, AzCosmosResponse<T>>(this, value);

            return result;
        }

        private void ThrowIfInvalid_response()
        {
            if (_response == null)
                ExThrower.ST_ThrowInvalidOperationException("Internal Response is null");
        }
    }
}
