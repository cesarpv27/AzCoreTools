using AzCoreTools.Core.Interfaces;
using AzCoreTools.Core.Validators;
using AzCoreTools.Helpers;
using Azure;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Core
{
    public class AzStorageResponse<T> : Response<T>, IAzStorageResponse
    {
        protected internal Response _response;
        protected T _value;

        public virtual bool Succeeded { get; protected internal set; }
        public virtual Exception Exception { get; protected internal set; }
        public virtual string Message { get; protected internal set; }

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

        private void InitializeWithoutValidations<TIn>(TIn response, T value) where TIn : Response
        {
            _response = response;
            _value = value;

            if (_response != null)
                Succeeded = ResponseValidator.ResponseSucceeded(_response);
        }

        protected virtual void Initialize<TIn>(TIn response, T value) where TIn : Response
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response, value);
        }

        protected virtual void Initialize<TIn>(TIn response) where TIn : Response<T>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            Initialize(response.GetRawResponse(), response.Value);
        }

        protected virtual void Initialize(T value)
        {
            InitializeWithoutValidations<Response>(default, value);
        }

        protected virtual void InitializeWithException<TEx>(TEx exception) where TEx : Exception
        {
            AzCoreHelper.TryInitialize(exception, this, null);
        }

        protected virtual void Initialize<GenTSource, TSource>(TSource source) where TSource : AzStorageResponse<GenTSource>, new()
        {
            InitializeWithoutValidations(source._response, default);
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

        public static AzStorageResponse<T> Create<TIn>(TIn response, T value) where TIn : Response
        {
            return Create<TIn, AzStorageResponse<T>>(response, value);
        }

        public static TOut Create<TIn, TOut>(TIn response, T value) where TIn : Response where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response, value);

            return result;
        }

        public static AzStorageResponse<T> Create<TIn>(TIn response) where TIn : Response<T>
        {
            return Create<TIn, AzStorageResponse<T>>(response);
        }

        public static TOut Create<TIn, TOut>(TIn response) where TIn : Response<T> where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

            return result;
        }

        public static AzStorageResponse<T> Create(T value)
        {
            return Create<AzStorageResponse<T>>(value);
        }

        public static TOut Create<TOut>(T value) where TOut : AzStorageResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(value);

            return result;
        }

        public static AzStorageResponse<T> Create(Exception exception)
        {
            return Create<AzStorageResponse<T>>(exception);
        }

        public static TOut Create<TOut>(Exception exception) where TOut : AzStorageResponse<T>, new()
        {
            return CreateWithException<Exception, TOut>(exception);
        }
        
        public static TOut CreateWithException<TEx, TOut>(TEx exception) where TEx : Exception where TOut : AzStorageResponse<T>, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(exception, nameof(exception));
            
            var result = CreateNew<TOut>();
            result.InitializeWithException(exception);

            return result;
        }

        #endregion

        #region Overridden

        public override T Value
        {
            get
            {
                return _value;
            }
        }

        public override Response GetRawResponse() => _response;

        #endregion

        public virtual AzStorageResponse<GenTOut> InduceResponse<GenTOut>()
        {
            return InduceResponse<GenTOut, AzStorageResponse<GenTOut>>();
        }

        public virtual TOut InduceResponse<GenTOut, TOut>() where TOut : AzStorageResponse<GenTOut>, new()
        {
            var result = AzStorageResponse<GenTOut>.CreateNew<TOut>();

            result.Initialize<T, AzStorageResponse<T>>(this);

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

        public virtual bool Succeeded { get; protected internal set; }
        public virtual Exception Exception { get; protected internal set; }
        public virtual string Message { get; protected internal set; }

        #region Constructors

        public AzStorageResponse() { }

        public AzStorageResponse(Response response)
        {
            Initialize(response);
        }

        #endregion

        #region Initializers

        internal void InitializeWithoutValidations<TIn>(TIn response) where TIn : Response
        {
            _response = response;

            if (_response != null)
                Succeeded =  ResponseValidator.ResponseSucceeded(_response);
        }

        protected virtual void Initialize<TIn>(TIn response) where TIn : Response
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            InitializeWithoutValidations(response);
        }

        protected virtual void Initialize<TIn, GenTIn>(TIn response) where TIn : Response<GenTIn>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            Initialize(response.GetRawResponse());
        }

        protected virtual void Initialize(RequestFailedException rfException)
        {
            AzCoreHelper.TryInitialize<bool>(rfException, null, this);
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

        public static AzStorageResponse Create<TIn>(TIn response) where TIn : Response
        {
            return Create(response);
        }

        public static TOut Create<TIn, TOut>(TIn response) where TIn : Response where TOut : AzStorageResponse, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize(response);

            return result;
        }

        public static AzStorageResponse Create1<TIn, GenTIn>(TIn response) where TIn : Response<GenTIn>
        {
            return Create<TIn, GenTIn, AzStorageResponse>(response);
        }

        public static TOut Create<TIn, GenTIn, TOut>(TIn response) where TIn : Response<GenTIn> where TOut : AzStorageResponse, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize<TIn, GenTIn>(response);

            return result;
        }

        public static AzStorageResponse Create(RequestFailedException rfException)
        {
            return Create<AzStorageResponse>(rfException);
        }

        public static TOut Create<TOut>(RequestFailedException rfException) where TOut : AzStorageResponse, new()
        {
            ExThrower.ST_ThrowIfArgumentIsNull(rfException, nameof(rfException));

            var result = CreateNew<TOut>();
            result.Initialize(rfException);

            return result;
        }

        #endregion

        #region Overridden

        public override int Status
        {
            get
            {
                ThrowIfInvalid_response();

                return _response.Status;
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
                ExThrower.ST_ThrowInvalidOperationException("Internal response is null");
        }
    }
}
