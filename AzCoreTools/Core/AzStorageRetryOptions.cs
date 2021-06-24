using Azure.Core;
using System;
using System.Collections.Generic;
using System.Text;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Core
{
    public class AzStorageRetryOptions
    {
        /// <summary>
        /// The maximum number of retry attempts before giving up.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// The delay between retry attempts for a fixed approach or the delay on which to
        /// base calculations for a backoff-based approach.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// The maximum permissible delay between retry attempts.
        /// </summary>
        public TimeSpan MaxDelay { get; set; }

        /// <summary>
        /// The approach to use for calculating retry delays.
        /// </summary>
        public RetryMode Mode { get; set; }

        /// <summary>
        /// The timeout applied to an individual network operations.
        /// </summary>
        public TimeSpan NetworkTimeout { get; set; }

        public virtual void CopyTo(RetryOptions retryOpt)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(retryOpt, nameof(retryOpt));

            retryOpt.MaxRetries = MaxRetries;
            retryOpt.Delay = Delay;
            retryOpt.MaxDelay = MaxDelay;
            retryOpt.Mode = Mode;
            retryOpt.NetworkTimeout = NetworkTimeout;
        }
    }
}
