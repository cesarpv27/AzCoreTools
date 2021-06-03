using Azure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Core.Validators
{
    public class ResponseValidator
    {
        public static bool CreateResourceResponseSucceeded<T>(Response<T> response)
        {
            return response == null || ResponseSucceeded(response);
        }

        public static bool ResponseSucceeded<T>(Response<T> response)
        {
            var rawResponse = response.GetRawResponse();
            if (!IsValidStatus(rawResponse.Status))
                return false;

            return StatusSucceeded((HttpStatusCode)rawResponse.Status);
        }

        public static bool ResponseSucceeded(Response response)
        {
            if (!IsValidStatus(response.Status))
                return false;

            return StatusSucceeded((HttpStatusCode)response.Status);
        }

        public static bool StatusSucceeded(int status)
        {
            if (!IsValidStatus(status))
                return false;

            return StatusSucceeded((HttpStatusCode)status);
        }
        
        public static bool StatusSucceeded(HttpStatusCode httpStatusCode)
        {
            switch (httpStatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.NotModified:
                case HttpStatusCode.Found:
                case HttpStatusCode.Continue:
                case HttpStatusCode.Moved:
                //case HttpStatusCode.MovedPermanently:
                //case HttpStatusCode.Redirect:
                case HttpStatusCode.TemporaryRedirect:
                case HttpStatusCode.RedirectMethod:
                //case HttpStatusCode.RedirectKeepVerb:
                case HttpStatusCode.PartialContent:
                case HttpStatusCode.ResetContent:
                //case HttpStatusCode.SeeOther:
                case HttpStatusCode.SwitchingProtocols:
                case HttpStatusCode.Unused:
                    return true;

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.BadGateway:
                //case HttpStatusCode.Ambiguous:
                case HttpStatusCode.Conflict:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.ExpectationFailed:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotAcceptable:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.PreconditionFailed:
                case HttpStatusCode.NonAuthoritativeInformation:
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.Gone:
                case HttpStatusCode.HttpVersionNotSupported:
                case HttpStatusCode.LengthRequired:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.UnsupportedMediaType:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.ProxyAuthenticationRequired:
                case HttpStatusCode.MultipleChoices:
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                case HttpStatusCode.RequestEntityTooLarge:
                case HttpStatusCode.RequestUriTooLong:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.UpgradeRequired:
                case HttpStatusCode.UseProxy:
                    return false;

                default:
                    ExThrower.ST_ThrowArgumentOutOfRangeException(httpStatusCode);
                    return false;
            }
        }

        private static bool IsValidStatus(int status)
        {
            return Enum.IsDefined(typeof(HttpStatusCode), status);
        }
    }
}
