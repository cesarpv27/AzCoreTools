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

        protected virtual void InitializeWithException<TEx>(TEx exception) where TEx : Exception
        {
            AzCoreHelper.TryInitialize(exception, this);
        }

        #endregion

        #region Creators

        public static TOut CreateNew<TOut>() where TOut : AzCosmosResponse<T>, new()
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

        public override T Resource
        {
            get
            {
                return _value;
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

        private void ThrowIfInvalid_response()
        {
            if (_response == null)
                ExThrower.ST_ThrowInvalidOperationException("Internal Response is null");
        }
    }
}
