using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Core.Interfaces
{
    public interface IAzErrorManager
    {
        bool IsResourceAlreadyExistsException<TEx>(TEx exception) where TEx : Exception;
        bool IsResourceNotFoundException<TEx>(TEx exception) where TEx : Exception;
    }
}
