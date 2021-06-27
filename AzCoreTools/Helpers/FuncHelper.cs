using AzCoreTools.Core;
using Azure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Helpers
{
    enum ByParamsFuncType
    {
        _1param = 1,
        _2params = 2,
        _3params = 3,
        _4params = 4,
        _5params = 5,
        _6params = 6,
    }

    public partial class FuncHelper // Commons & generic response
    {
        #region Commons

        private static void TryCastAs_ByParamsFuncType_OrThrows(dynamic[] funcParams, out ByParamsFuncType byParamsFuncType)
        {
            if (!CoreTools.Helpers.Helper.EnumContains<ByParamsFuncType>(funcParams.Length))
                ExThrower.ST_ThrowArgumentOutOfRangeException(nameof(funcParams));
            
            byParamsFuncType = (ByParamsFuncType)funcParams.Length;
        }

        private static FTOut ExecuteFunc<FTOut>(
            dynamic func,
            dynamic[] funcParams)
        {
            ByParamsFuncType byParamsFuncType;
            TryCastAs_ByParamsFuncType_OrThrows(funcParams, out byParamsFuncType);

            FTOut funcResponse;
            switch (byParamsFuncType)
            {
                case ByParamsFuncType._1param:
                    funcResponse = func(funcParams[0]);
                    break;
                case ByParamsFuncType._2params:
                    funcResponse = func(funcParams[0], funcParams[1]);
                    break;
                case ByParamsFuncType._3params:
                    funcResponse = func(funcParams[0], funcParams[1], funcParams[2]);
                    break;
                case ByParamsFuncType._4params:
                    funcResponse = func(funcParams[0], funcParams[1], funcParams[2], funcParams[3]);
                    break;
                case ByParamsFuncType._5params:
                    funcResponse = func(funcParams[0], funcParams[1], funcParams[2], funcParams[3], funcParams[4]);
                    break;
                case ByParamsFuncType._6params:
                    funcResponse = func(funcParams[0], funcParams[1], funcParams[2], funcParams[3], funcParams[4], funcParams[5]);
                    break;
                default:
                    ExThrower.ST_ThrowArgumentOutOfRangeException(byParamsFuncType);
                    return default;
            }

            return funcResponse;
        }

        private static async Task<FTOut> ExecuteFuncAsync<FTOut>(
            dynamic func,
            dynamic[] funcParams)
        {
            ByParamsFuncType byParamsFuncType;
            TryCastAs_ByParamsFuncType_OrThrows(funcParams, out byParamsFuncType);

            FTOut funcResponse;

            switch (byParamsFuncType)
            {
                case ByParamsFuncType._1param:
                    funcResponse = await func(funcParams[0]);
                    break;
                case ByParamsFuncType._2params:
                    funcResponse = await func(funcParams[0], funcParams[1]);
                    break;
                case ByParamsFuncType._3params:
                    funcResponse = await func(funcParams[0], funcParams[1], funcParams[2]);
                    break;
                case ByParamsFuncType._4params:
                    funcResponse = await func(funcParams[0], funcParams[1], funcParams[2], funcParams[3]);
                    break;
                case ByParamsFuncType._5params:
                    funcResponse = await func(funcParams[0], funcParams[1], funcParams[2], funcParams[3], funcParams[4]);
                    break;
                case ByParamsFuncType._6params:
                    funcResponse = await func(funcParams[0], funcParams[1], funcParams[2], funcParams[3], funcParams[4], funcParams[5]);
                    break;
                default:
                    ExThrower.ST_ThrowArgumentOutOfRangeException(byParamsFuncType);
                    return default;
            }

            return funcResponse;
        }

        #endregion

        #region private Execute with GenTOut

        private static TOut Execute<FTOut, TOut, GenTOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            try
            {
                FTOut funcResponse = ExecuteFunc<FTOut>(func, funcParams);

                return AzStorageResponse<GenTOut>.Create<FTOut, TOut>(funcResponse);
            }
            catch (RequestFailedException e)
            {
                return AzStorageResponse<GenTOut>.Create<TOut>(e);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static async Task<TOut> ExecuteAsync<FTOut, TOut, GenTOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            try
            {
                FTOut funcResponse = await ExecuteFuncAsync(func, funcParams);

                return AzStorageResponse<GenTOut>.Create<FTOut, TOut>(funcResponse);
            }
            catch (RequestFailedException e)
            {
                return AzStorageResponse<GenTOut>.Create<TOut>(e);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Func execute - 2 params

        public static TOut Execute<FTIn1, FTIn2, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTOut> func, FTIn1 FTParam1, FTIn2 FTParam2)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return Execute<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, Task<FTOut>> func, FTIn1 FTParam1, FTIn2 FTParam2)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return await ExecuteAsync<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2
            });
        }

        #endregion

        #region Func execute - 3 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTOut> func, FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return Execute<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, Task<FTOut>> func, FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return await ExecuteAsync<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3
            });
        }

        #endregion

        #region Func execute - 4 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
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
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
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

        #region Func execute - 5 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return Execute<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return await ExecuteAsync<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5
            });
        }

        #endregion

        #region Func execute - 6 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5, FTIn6 FTParam6)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return Execute<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
                FTParam6
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut, TOut, GenTOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5, FTIn6 FTParam6)
            where FTOut : Response<GenTOut> where TOut : AzStorageResponse<GenTOut>, new()
        {
            return await ExecuteAsync<FTOut, TOut, GenTOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
                FTParam6
            });
        }

        #endregion
    }


    public partial class FuncHelper
    {
        #region private Execute

        private static TOut Execute<FTOut, TOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            try
            {
                FTOut funcResponse = ExecuteFunc<FTOut>(func, funcParams);

                return AzStorageResponse.Create<FTOut, TOut>(funcResponse);
            }
            catch (RequestFailedException e)
            {
                return AzStorageResponse.Create<TOut>(e);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task<TOut> ExecuteAsync<FTOut, TOut>(
            dynamic func,
            dynamic[] funcParams)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            try
            {
                FTOut funcResponse = await ExecuteFuncAsync<FTOut>(func, funcParams);

                return AzStorageResponse.Create<FTOut, TOut>(funcResponse);
            }
            catch (RequestFailedException e)
            {
                return AzStorageResponse.Create<TOut>(e);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Func execute - 1 param

        public static TOut Execute<FTIn, FTOut, TOut>(
            Func<FTIn, FTOut> func, FTIn FTParam)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn, FTOut, TOut>(
            Func<FTIn, Task<FTOut>> func, FTIn FTParam)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam
            });
        }

        #endregion

        #region Func execute - 2 params

        public static TOut Execute<FTIn1, FTIn2, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTOut> func, FTIn1 FTParam1, FTIn2 FTParam2) 
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTOut, TOut>(
            Func<FTIn1, FTIn2, Task<FTOut>> func, FTIn1 FTParam1, FTIn2 FTParam2)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2
            });
        }

        #endregion

        #region Func execute with - 3 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTOut> func, FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, Task<FTOut>> func, FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3
            });
        }

        #endregion

        #region Func execute with - 4 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
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

        #region Func execute with - 5 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn4, FTIn5, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
            });
        }

        #endregion

        #region Func execute with - 6 params

        public static TOut Execute<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5, FTIn6 FTParam6)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return Execute<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
                FTParam6,
            });
        }

        public static async Task<TOut> ExecuteAsync<FTIn1, FTIn2, FTIn3, FTIn4, FTIn5, FTIn6, FTOut, TOut>(
            Func<FTIn1, FTIn2, FTIn3, FTIn4, FTIn4, FTIn5, FTIn6, Task<FTOut>> func,
            FTIn1 FTParam1, FTIn2 FTParam2, FTIn3 FTParam3, FTIn4 FTParam4, FTIn5 FTParam5, FTIn6 FTParam6)
            where FTOut : Response where TOut : AzStorageResponse, new()
        {
            return await ExecuteAsync<FTOut, TOut>(
            func,
            new dynamic[]
            {
                FTParam1,
                FTParam2,
                FTParam3,
                FTParam4,
                FTParam5,
                FTParam6,
            });
        }

        #endregion

    }
}
