using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
using AzCoreTools.Core.Validators;
using AzCoreTools.Core.Interfaces;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using AzCoreTools.Helpers;
using System.Net;

namespace AzCoreTools.Core
{
    public class AzCosmosResponse<T> : Response<T>, IAzCosmosResponse<T>
    {
        protected internal Response<T> _response;
        protected T _value;

        public virtual bool Succeeded { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual string Message { get; set; }

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

        private void InitializeWithoutValidations<RTIn>(RTIn response, T value) where RTIn : Response<T>
        {
            _response = response;
            _value = value;

            if (_response != null)
                Succeeded = ResponseValidator.CosmosResponseSucceeded<Response<T>, T>(_response);
        }
        
        protected virtual void Initialize<RTIn>(RTIn response, T value) where RTIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response, value);
        }

        protected virtual void Initialize<RTIn>(RTIn response) where RTIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            Initialize(response, response.Resource);
        }

        protected virtual void Initialize(T value)
        {
            InitializeWithoutValidations<Response<T>>(default, value);
        }

        protected virtual void InitializeWithException<TEx>(TEx exception) where TEx : Exception
        {
            AzCoreHelper.TryInitialize(exception, this);
        }

        protected virtual void Initialize<GenT, RTSource>(RTSource source, T value = default) where RTSource : AzCosmosResponse<GenT>, new()
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

        public static AzCosmosResponse<T> Create<RTIn>(RTIn response) where RTIn : Response<T>
        {
            return Create<RTIn, AzCosmosResponse<T>>(response);
        }

        public static TOut Create<RTIn, TOut>(RTIn response) where RTIn : Response<T> where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

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

        public static AzCosmosResponse<T> Create(T value, bool succeeded)
        {
            var result = Create<AzCosmosResponse<T>>(value);
            result.Succeeded = succeeded;

            return result;
        }

        public static TOut Create<TOut>(T value) where TOut : AzCosmosResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(value);

            return result;
        }

        public static TOut Create<TOut>(Exception exception) where TOut : AzCosmosResponse<T>, new()
        {
            return CreateWithException<Exception, TOut>(exception);
        }

        public static TOut CreateWithException<TEx, TOut>(TEx exception) where TEx : Exception where TOut : AzCosmosResponse<T>, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(exception, nameof(exception));

            var result = CreateNew<TOut>();
            result.InitializeWithException(exception);

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
