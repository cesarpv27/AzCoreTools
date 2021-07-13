using AzCoreTools.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace AzCoreTools.Helpers
{
    public class CosmosFuncHelper
    {
        #region private Execute with GenTOut

        private static TOut Execute<FTOut, TOut, GenTOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response<GenTOut> where TOut : AzCosmosResponse<GenTOut>, new()
        {
            try
            {
                FTOut funcResponse = FuncHelper.ExecuteFunc<FTOut>(func, funcParams);

                return AzCosmosResponse<GenTOut>.Create<FTOut, TOut>(funcResponse);
            }
            catch (Exception e)
            {
                return AzCosmosResponse<GenTOut>.Create<TOut>(e);
            }
        }

        private static async Task<TOut> ExecuteAsync<FTOut, TOut, GenTOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response<GenTOut> where TOut : AzCosmosResponse<GenTOut>, new()
        {
            try
            {
                FTOut funcResponse = await FuncHelper.ExecuteFuncAsync<FTOut>(func, funcParams);

                return AzCosmosResponse<GenTOut>.Create<FTOut, TOut>(funcResponse);
            }
            catch (Exception e)
            {
                return AzCosmosResponse<GenTOut>.Create<TOut>(e);
            }
        }

        #endregion

        #region Func execute - 4 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4)
            where FTOut : Response<GenTOut> where TOut : AzCosmosResponse<GenTOut>, new()
        {
            return Execute<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4)
            where FTOut : Response<GenTOut> where TOut : AzCosmosResponse<GenTOut>, new()
        {
            return await ExecuteAsync<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4
            });
        }

        #endregion

    }
}
