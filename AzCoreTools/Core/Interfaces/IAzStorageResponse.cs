using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Core.Interfaces
{
    public interface IAzStorageResponse : IAzResponse
    {
        Exception Exception { get; }
        string Message { get; }
    }
}
