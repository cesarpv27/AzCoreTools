using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using ExThrower = CoreTools.Throws.ExceptionThrower;
using System.Threading.Tasks;

namespace AzCoreTools.Core
{
    public class AzExtensionTools
    {
        internal static AzStorageResponse<List<T>> TakeFromPageable<T>(
            AzStorageResponse<Pageable<T>> response,
            int take)
        {
            if (!TakeFromPageable_ValidateParams(response, take))
                return response.InduceResponse<List<T>>();

            var result = new List<T>(Math.Min(take, 1000));
            var count = 0;
            foreach (var item in response.Value)
            {
                result.Add(item);

                if (++count >= take)
                    return AzStorageResponse<List<T>>.Create(result, true);
            }

            return AzStorageResponse<List<T>>.Create(result, true);
        }

        internal static async Task<AzStorageResponse<List<T>>> TakeFromPageableAsync<T>(
            AzStorageResponse<AsyncPageable<T>> response,
            int take)
        {
            if (!TakeFromPageable_ValidateParams(response, take))
                return response.InduceResponse<List<T>>();

            var result = new List<T>(Math.Min(take, 1000));
            var count = 0;

            var enumerator = response.Value.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    result.Add(enumerator.Current);

                    if (++count >= take)
                        return AzStorageResponse<List<T>>.Create(result, true);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            // TODO: In C# 8.0
            //await foreach (var item in response.Value) { ... }

            return AzStorageResponse<List<T>>.Create(result, true);
        }

        private static bool TakeFromPageable_ValidateParams<TValue>(
            AzStorageResponse<TValue> response,
            int take)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(response, nameof(response));
            if (take <= 0)
                ExThrower.ST_ThrowArgumentException($"'{nameof(take)}' must be greater than zero");

            if (!response.Succeeded)
                return false;

            ExThrower.ST_ThrowIfArgumentIsNull(response.Value, nameof(response), "response.Value is null");

            return true;
        }
    }
}
