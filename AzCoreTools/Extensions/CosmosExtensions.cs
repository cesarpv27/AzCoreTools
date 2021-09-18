using AzCoreTools.Core;
using CoreTools.Extensions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Extensions
{
    public static class CosmosExtensions
    {
        public static T ConvertFromResponse<T>(this ItemResponse<T> @this) where T : class
        {
            // TODO: Verificar si se debe restringir el tipo T a una clase especifica
            if (@this.Resource == null)
                return null;
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(@this.Resource));
        }

        #region Cosmos entity to custom type

        public static List<AzCosmosResponse<List<T>>> DeserializeObjects<T>(
            this List<AzCosmosResponse<List<string>>> @this)
            where T : class
        {
            return @this.Select(ent => ent.DeserializeObjects<T>()).ToList();
        }
        
        public static AzCosmosResponse<List<T>> DeserializeObjects<T>(
            this AzCosmosResponse<List<string>> azCosmosResponse)
            where T : class
        {
            if (!azCosmosResponse.Succeeded)
                ExThrower.ST_ThrowInvalidOperationException($"{nameof(azCosmosResponse)} is not successful.");
            if (azCosmosResponse.Value == null)
                ExThrower.ST_ThrowInvalidOperationException($"{nameof(azCosmosResponse)} has null value.");

            return azCosmosResponse.InduceResponse(azCosmosResponse.Value.DeserializeObjects<T>());
        }

        public static List<T> DeserializeObjects<T>(this List<string> @this) where T : class
        {
            return @this.Select(ent => JsonConvert.DeserializeObject<T>(ent)).ToList();
        }

        #endregion

        #region Cosmos entity to string

        public static List<AzCosmosResponse<List<string>>> InduceResponsesToValueStrings(
            this List<AzCosmosResponse<TransactionalBatchResponse>> @this)
        {
            var _azCosmosResponses = new List<AzCosmosResponse<List<string>>>(@this.Count);
            foreach (var azCosmosResp in @this)
                _azCosmosResponses.Add(azCosmosResp.InduceResponseToValueStrings());

            return _azCosmosResponses;
        }

        public static AzCosmosResponse<List<string>> InduceResponseToValueStrings(
            this AzCosmosResponse<TransactionalBatchResponse> @this)
        {
            if (!@this.Succeeded)
                return @this.InduceResponse<List<string>>();
            if (@this.Value == default)
                ExThrower.ST_ThrowApplicationException($"Value of property '{nameof(@this.Value)}'is null");

            var strList = new List<string>(@this.Value.Count);
            string commonStr;
            foreach (var opResult in @this.Value)
            {
                if (!opResult.TryConvertResourceToString(out commonStr) || string.IsNullOrEmpty(commonStr))
                    ExThrower.ST_ThrowApplicationException("Could not convert response resource to string." +
                        $"Response ActivityId:{@this.Value.ActivityId}");
                else
                    strList.Add(commonStr);
            }

            return @this.InduceResponse(strList);
        }

        public static bool TryConvertResourceToString(this TransactionalBatchOperationResult @this, out string value)
        {
            try
            {
                value = @this.ConvertResourceToString();
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static string ConvertResourceToString(this TransactionalBatchOperationResult @this)
        {
            if (@this.ResourceStream == default)
                ExThrower.ST_ThrowInvalidOperationException($"Value of property '{nameof(@this.ResourceStream)}' is null");

            using (StreamReader streamReader = new StreamReader(@this.ResourceStream))
            {
                return streamReader.ReadToEndAsync().WaitAndUnwrapException();
            }
        }

        #endregion

    }
}
