using System;
using System.Collections.Generic;
using System.Text;

namespace AzCoreTools.Texting
{
    public static class AzTextingResources
    {
        public const string PartitionKeyName = "PartitionKey";
        public const string RowKeyName = "RowKey";
        public const string TimestampName = "Timestamp";
        public const string More_than_one_entity_found = "More than one entities found";
        public const string Exception_message = "Exception message: ";
        public static string Param_must_be_grather_than_zero(string paramName)
        {
            return $"Parameter '{paramName}' must be greater than zero";
        }
    }
}
