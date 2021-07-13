using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Core.Interfaces
{
    public interface IAzStorageResponse : IAzDetailedResponse
    {
    }

    public interface IAzStorageResponse<T> : IAzDetailedResponse<T>
    {
    }
}
