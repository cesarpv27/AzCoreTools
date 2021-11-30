using CoreTools.Throws;
using System;
using System.Collections.Generic;
using System.Text;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Throws
{
    public class AzExceptionThrower : ExThrower
    {
        #region Properties

        private static AzExceptionThrower _azExThrower;
        private static AzExceptionThrower AzExThrower
        {
            get
            {
                if (_azExThrower == null)
                    _azExThrower = new AzExceptionThrower();

                return _azExThrower;
            }
        }

        #endregion

        #region Static throw's methods

        public static void ST_ThrowIfPartitionKeyIsInvalid(string partitionKey, string paramName = null, string message = null)
        {
            AzExThrower.ThrowIfPartitionKeyIsInvalid(partitionKey, paramName, message);
        }

        public static void ST_ThrowIfRowKeyIsInvalid(string rowKey, string paramName = null, string message = null)
        {
            AzExThrower.ThrowIfRowKeyIsInvalid(rowKey, paramName, message);
        }

        #endregion

        #region Throw's methods

        public virtual void ThrowIfPartitionKeyIsInvalid(string partitionKey, string paramName, string message)
        {
            ThrowIfKeyIsInvalid(partitionKey, paramName, message);
        }

        public virtual void ThrowIfRowKeyIsInvalid(string rowKey, string paramName, string message)
        {
            ThrowIfKeyIsInvalid(rowKey, paramName, message);
        }

        public virtual void ThrowIfKeyIsInvalid(string key, string paramName, string message)
        {
            AzExThrower.ThrowIfArgumentIsNullOrWhitespace(key, paramName, message);
        }

        #endregion
    }
}
