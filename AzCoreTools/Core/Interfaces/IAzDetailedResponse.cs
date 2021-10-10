using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Core.Interfaces
{
    public interface IAzDetailedResponse : IAzResponse
    {
        Exception Exception { get; set; }
        string Message { get; set; }
    }

    public interface IAzDetailedResponse<T> : IAzDetailedResponse
    {
        T Value { get; }
        string ContinuationToken { get; set; }
    }
}
