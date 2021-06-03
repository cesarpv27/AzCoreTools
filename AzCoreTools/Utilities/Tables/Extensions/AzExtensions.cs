using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Utilities.Tables.Extensions
{
    public static class AzExtensions
    {
        public static FilterCondition GenerateFilterCondition(this string @this,
            QueryComparison operation,
            string value)
        {
            return new FilterCondition(@this, operation, value);
        }

        public static FilterCondition GenerateFilterCondition(this string @this,
            QueryComparison operation,
            DateTime value)
        {
            return new FilterCondition(@this, operation, value);
        }
    }
}
