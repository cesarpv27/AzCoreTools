using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
