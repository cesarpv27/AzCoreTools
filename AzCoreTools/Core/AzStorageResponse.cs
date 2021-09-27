using AzCoreTools.Core.Interfaces;
using AzCoreTools.Core.Validators;
using AzCoreTools.Helpers;
using AzCoreTools.Texting;
using Azure;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Core
{
    public class AzStorageResponse<T> : Response<T>, IAzStorageResponse<T>
    {
        protected internal Response _response;
        protected T _value;

        public virtual bool Succeeded { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual string Message { get; set; }

        #region Constructors

        public AzStorageResponse()
        {
            _value = default;
            _response = default;
        }

        public AzStorageResponse(Response response, T value)
        {
            Initialize(response, value);
        }

        public AzStorageResponse(Response<T> response)
        {
            Initialize(response);
        }

        protected AzStorageResponse(T value)
        {
            Initialize(value);
        }

        #endregion

        #region Initializers

        private void InitializeWithoutValidations<RTIn>(RTIn response, T value) where RTIn : Response
        {
            _response = response;
            _value = value;

            if (_response != null)
                Succeeded = ResponseValidator.ResponseSucceeded<Response>(_response);
        }

        protected virtual void Initialize<RTIn>(RTIn response, T value) where RTIn : Response
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response, value);
        }

        protected virtual void Initialize<RTIn>(RTIn response) where RTIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            var rawResponse = response.GetRawResponse();
            if (rawResponse != default)
                Initialize(rawResponse, response.Value);
            else
                InitializeWithoutValidations(default(Response), response.Value);
        }

        protected virtual void Initialize(T value)
        {
            InitializeWithoutValidations<Response>(default, value);
        }

        protected virtual void InitializeWithException<TEx>(
            TEx exception, 
            string messagePrefix = AzTextingResources.Exception_message) 
            where TEx : Exception
        {
            AzCoreHelper.TryInitialize(exception, this, messagePrefix);
        }

        protected virtual void Initialize<GenT, RTSource>(RTSource source, T value = default) 
            where RTSource : AzStorageResponse<GenT>, new()
        {
            InitializeWithoutValidations(source._response, value);
            Succeeded = source.Succeeded;
            Message = source.Message;
            Exception = source.Exception;
        }

        #endregion

        #region Creators

        public static TOut CreateNew<TOut>() where TOut : AzStorageResponse<T>, new()
        {
            return new TOut();
        }

        public static TOut CreateNew<GenT, TOut>() where TOut : AzStorageResponse<GenT>, new()
        {
            return new TOut();
        }

        public static AzStorageResponse<T> Create<RTIn>(RTIn response, T value) where RTIn : Response
        {
            return Create<RTIn, AzStorageResponse<T>>(response, value);
        }

        public static TOut Create<RTIn, TOut>(RTIn response, T value) where RTIn : Response where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response, value);

            return result;
        }

        public static AzStorageResponse<T> Create<RTIn>(RTIn response) where RTIn : Response<T>
        {
            return Create<RTIn, AzStorageResponse<T>>(response);
        }

        public static TOut Create<RTIn, TOut>(RTIn response) where RTIn : Response<T> where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

            return result;
        }

        public static AzStorageResponse<T> Create(
            T value, 
            bool succeeded,
            string message = default)
        {
            return Create<AzStorageResponse<T>>(value, succeeded, message);
        }

        public static TOut Create<TOut>(
            T value, 
            bool succeeded,
            string message = default) 
            where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(value);

            result.Succeeded = succeeded;
            result.Message = message;

            return result;
        }

        public static AzStorageResponse<T> Create(
            Exception exception, 
            string messagePrefix = AzTextingResources.Exception_message)
        {
            return Create<AzStorageResponse<T>>(exception, messagePrefix);
        }

        public static TOut Create<TOut>(
            Exception exception, 
            string messagePrefix = AzTextingResources.Exception_message) 
            where TOut : AzStorageResponse<T>, new()
        {
            return CreateWithException<Exception, TOut>(exception, messagePrefix);
        }
        
        public static TOut CreateWithException<TEx, TOut>(
            TEx exception, 
            string messagePrefix = AzTextingResources.Exception_message) 
            where TEx : Exception 
            where TOut : AzStorageResponse<T>, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(exception, nameof(exception));
            
            var result = CreateNew<TOut>();
            result.InitializeWithException(exception, messagePrefix);

            return result;
        }
        
        #endregion

        #region Abstract class implementation

        public override T Value
        {
            get
            {
                return _value;
            }
        }

        public override Response GetRawResponse()
        {
            if (_response == null)
                _response = InduceGenericLessResponse();

            return _response;
        }

        #endregion

        public virtual AzStorageResponse<GenTOut> InduceResponse<GenTOut>(GenTOut value = default)
        {
            return InduceResponse<GenTOut, AzStorageResponse<GenTOut>>(value);
        }

        public virtual TOut InduceResponse<GenTOut, TOut>(GenTOut value = default) 
            where TOut : AzStorageResponse<GenTOut>, new()
        {
            var result = AzStorageResponse<GenTOut>.CreateNew<TOut>();

            result.Initialize<T, AzStorageResponse<T>>(this, value);

            return result;
        }

        public virtual AzStorageResponse InduceGenericLessResponse()
        {
            return InduceGenericLessResponse<AzStorageResponse>();
        }

        public virtual TOut InduceGenericLessResponse<TOut>() where TOut : AzStorageResponse, new()
        {
            var result = AzStorageResponse.CreateNew<TOut>();
            result.Initialize<T, AzStorageResponse<T>>(this);

            return result;
        }
    }

    public class AzStorageResponse : Response, IAzStorageResponse
    {
        protected Response _response;

        public virtual bool Succeeded { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual string Message { get; set; }

        #region Constructors

        public AzStorageResponse() { }

        public AzStorageResponse(Response response)
        {
            Initialize(response);
        }

        #endregion

        #region Initializers

        internal void InitializeWithoutValidations<RTIn>(RTIn response) where RTIn : Response
        {
            _response = response;

            if (_response != null)
                Succeeded =  ResponseValidator.ResponseSucceeded<Response>(_response);
        }

        protected virtual void Initialize<RTIn>(RTIn response) where RTIn : Response
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response);
        }

        protected virtual void Initialize<RTIn, GenTIn>(RTIn response) where RTIn : Response<GenTIn>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            Initialize(response.GetRawResponse());
        }

        protected virtual void Initialize(
            Exception exception,
            string messagePrefix = AzTextingResources.Exception_message)
        {
            AzCoreHelper.TryInitialize(exception, this, messagePrefix);
        }

        protected internal virtual void Initialize<GenTSource, TSource>(TSource source) where TSource : AzStorageResponse<GenTSource>, new()
        {
            InitializeWithoutValidations(source._response);
            Succeeded = source.Succeeded;
            Message = source.Message;
            Exception = source.Exception;
        }

        #endregion

        #region Creators

        public static TOut CreateNew<TOut>() where TOut : AzStorageResponse, new()
        {
            return new TOut();
        }

        public static AzStorageResponse Create<RTIn>(RTIn response) where RTIn : Response
        {
            return Create(response);
        }

        public static TOut Create<RTIn, TOut>(RTIn response) where RTIn : Response where TOut : AzStorageResponse, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

            return result;
        }

        public static AzStorageResponse Create1<RTIn, GenTIn>(RTIn response) where RTIn : Response<GenTIn>
        {
            return Create<RTIn, GenTIn, AzStorageResponse>(response);
        }

        public static TOut Create<RTIn, GenTIn, TOut>(RTIn response) where RTIn : Response<GenTIn> where TOut : AzStorageResponse, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize<RTIn, GenTIn>(response);

            return result;
        }

        public static AzStorageResponse Create(
            Exception exception,
            string messagePrefix = AzTextingResources.Exception_message)
        {
            return Create<AzStorageResponse>(exception, messagePrefix);
        }

        public static TOut Create<TOut>(
            Exception exception,
            string messagePrefix = AzTextingResources.Exception_message) 
            where TOut : AzStorageResponse, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(exception, nameof(exception));

            var result = CreateNew<TOut>();
            result.Initialize(exception, messagePrefix);

            return result;
        }

        #endregion

        #region Abstract class implementation

        public override int Status
        {
            get
            {
                if (_response != null)
                    return _response.Status;
                if (Succeeded)
                    return CoreTools.Helpers.Helper.ConvertToInt(HttpStatusCode.OK);

                return CoreTools.Helpers.Helper.ConvertToInt(HttpStatusCode.Conflict);
            }
        }
        public override string ReasonPhrase
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.ReasonPhrase;
            }
        }
        public override Stream ContentStream
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.ContentStream;
            }
            set
            {
                _response.ContentStream = value;
            }
        }
        public override string ClientRequestId
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.ClientRequestId;
            }
            set
            {
                _response.ClientRequestId = value;
            }
        }

        public override void Dispose()
        {
            _response?.Dispose();
        }

        protected override bool ContainsHeader(string name)
        {
            ExThrower.ST_ThrowNotImplementedException("ContainsHeader");
            return false;
        }

        protected override IEnumerable<HttpHeader> EnumerateHeaders()
        {
            ExThrower.ST_ThrowNotImplementedException("EnumerateHeaders");
            return null;
        }

        protected override bool TryGetHeader(string name, out string value)
        {
            ExThrower.ST_ThrowNotImplementedException("TryGetHeader");
            value = string.Empty;
            return false;
        }

        protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
        {
            ExThrower.ST_ThrowNotImplementedException("TryGetHeaderValues");
            values = null;
            return false;
        }

        #endregion

        private void ThrowIfInvalid_response()
        {
            if (_response == null)
                ExThrower.ST_ThrowInvalidOperationException("Internal Response is null");
        }
    }
}
