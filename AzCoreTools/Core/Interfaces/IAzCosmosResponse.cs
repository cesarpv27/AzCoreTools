using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Core.Interfaces
{
    public interface IAzCosmosResponse<T> : IAzDetailedResponse<T>
    {
        string ContinuationToken { get; set; }
    }
}
