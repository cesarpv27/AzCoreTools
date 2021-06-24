using AzCoreTools.Core.Validators;
using Azure;
using System.IO;
using System.Net;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Core
{
    public class AzEmptyResponse<T> : AzStorageResponse<T>
    {
        #region Constructors

        public AzEmptyResponse() { }

        public AzEmptyResponse(int status, string reasonPhrase)
        {
            Initialize(status, reasonPhrase);
        }

        #endregion

        #region Initializers

        protected virtual void Initialize(int status, string reasonPhrase)
        {
            _value = default;
            _response = new AzEmptyResponse(status, reasonPhrase);

            Succeeded = ResponseValidator.StatusSucceeded(status);
        }

        protected virtual void Initialize<TIn, GenTIn>(TIn response) where TIn : Response<GenTIn>
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));

            var rawResponse = response.GetRawResponse();
            Initialize(rawResponse.Status, rawResponse.ReasonPhrase);
        }

        #endregion

        #region Creators

        public new static AzEmptyResponse<T> Create<TIn, GenTIn>(TIn response) where TIn : Response<GenTIn>
        {
            return Create<TIn, GenTIn, AzEmptyResponse<T>>(response);
        }

        public static TOut Create<TIn, GenTIn, TOut>(TIn response) where TIn : Response<GenTIn> where TOut : AzEmptyResponse<T>, new()
        {
            var result = CreateNew<TOut>();
            result.Initialize<TIn, GenTIn>(response);

            return result;
        }

        public static AzEmptyResponse<T> Create(HttpStatusCode status, string reasonPhrase = null)
        {
            return Create<AzEmptyResponse<T>>(status, reasonPhrase);
        }

        public static TOut Create<TOut>(HttpStatusCode status, string reasonPhrase = null) where TOut : AzEmptyResponse<T>, new()
        {
            if (string.IsNullOrEmpty(reasonPhrase))
                reasonPhrase = status.ToString();

            var result = CreateNew<TOut>();
            result.Initialize(CoreTools.Helpers.Helper.ConvertToInt(status), reasonPhrase);

            return result;
        }

        public static TOut CreateNotModified<TOut>() where TOut : AzEmptyResponse<T>, new()
        {
            return Create<TOut>(HttpStatusCode.NotModified);
        }

        public static AzEmptyResponse<T> CreateNotModified()
        {
            return CreateNotModified<AzEmptyResponse<T>>();
        }

        #endregion
    }

    class AzEmptyResponse : AzStorageResponse
    {
        #region Constructors

        public AzEmptyResponse() { }

        public AzEmptyResponse(int status, string reasonPhrase)
        {
            Initialize(status, reasonPhrase);
        }

        #endregion

        #region Initializers

        protected virtual void Initialize(int status, string reasonPhrase)
        {
            _status = status;
            _reasonPhrase = reasonPhrase;

            Succeeded = ResponseValidator.StatusSucceeded(status);
        }

        #endregion

        #region Overridden

        protected int _status;
        public override int Status
        {
            get
            {
                return _status;
            }
        }

        protected string _reasonPhrase;
        public override string ReasonPhrase
        {
            get
            {
                return _reasonPhrase;
            }
        }

        public override Stream ContentStream { get; set; }
        public override string ClientRequestId { get; set; }

        public override void Dispose()
        {
        }

        #endregion    
    }
}
