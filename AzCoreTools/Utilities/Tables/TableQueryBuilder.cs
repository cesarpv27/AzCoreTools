using AzCoreTools.Texting;
using CoreTools.Utilities;
using System;
using System.Globalization;
using System.Text;
using AzCoreTools.Utilities.Tables.Extensions;
using ExThrower = CoreTools.Throws.ExceptionThrower;

namespace AzCoreTools.Utilities.Tables
{
    public class TableQueryBuilder
    {
        public static CultureInfo CurrentCulture { get; } = CultureInfo.InvariantCulture;

        public static FilterCondition GeneratePartitionKeyFilterCondition(QueryComparison operation, string value)
        {
            return AzTextingResources.PartitionKeyName.GenerateFilterCondition(operation, value);
        }

        public static FilterCondition GenerateRowKeyFilterCondition(QueryComparison operation, string value)
        {
            return AzTextingResources.RowKeyName.GenerateFilterCondition(operation, value);
        }
        
        public static FilterCondition GenerateTimestampFilterCondition(QueryComparison operation, DateTime value)
        {
            return AzTextingResources.TimestampName.GenerateFilterCondition(operation, value);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation, 
            string value)
        {
            return GenerateFilterCondition(propName, operation, value ?? string.Empty, AzPropType.String);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            int value)
        {
            return GenerateFilterCondition(propName, operation, Convert.ToString(value, CurrentCulture), AzPropType.Int32);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            double value)
        {
            return GenerateFilterCondition(propName, operation, Convert.ToString(value, CurrentCulture), AzPropType.Double);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            long value)
        {
            return GenerateFilterCondition(propName, operation, Convert.ToString(value, CurrentCulture), AzPropType.Int64);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            bool value)
        {
            return GenerateFilterCondition(propName, operation, value ? "true" : "false", AzPropType.Boolean);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            byte[] value)
        {
            ExThrower.ST_ThrowIfArgumentIsNull(value);

            var stringBuilder = new StringBuilder();
            foreach (var num in value)
                stringBuilder.AppendFormat("{0:x2}", num);

            return GenerateFilterCondition(propName, operation, stringBuilder.ToString(), AzPropType.Binary);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            DateTimeOffset value)
        {
            return GenerateFilterCondition(propName, operation, value.UtcDateTime.ToString("o", CurrentCulture), AzPropType.DateTime);
        }

        public static string GenerateFilterCondition(
            string propName,
            QueryComparison operation,
            Guid value)
        {
            ExThrower.ST_ThrowIfGuidArgumentIsEmpty(value);

            return GenerateFilterCondition(propName, operation, value.ToString(), AzPropType.Guid);
        }

        private static string GenerateFilterCondition(string propName,
          QueryComparison operation, string value, 
          AzPropType azPropType)
        {
            ExThrower.ST_ThrowIfArgumentIsNullOrWhitespace(propName, nameof(propName), nameof(propName));

            string formattedValue;
            switch (azPropType)
            {
                case AzPropType.Binary:
                    formattedValue = string.Format(CurrentCulture, "X'{0}'", value);
                    break;
                case AzPropType.Boolean:
                case AzPropType.Int32:
                    formattedValue = value;
                    break;
                case AzPropType.Int64:
                    formattedValue = string.Format(CurrentCulture, "{0}L", value);
                    break;
                case AzPropType.Double:
                    if (!int.TryParse(value, out int _))
                        formattedValue = value;
                    else
                        formattedValue = string.Format(CurrentCulture, "{0}.0", value);
                    break;
                case AzPropType.Guid:
                    formattedValue = string.Format(CurrentCulture, "guid'{0}'", value);
                    break;
                case AzPropType.DateTime:
                    formattedValue = string.Format(CurrentCulture, "datetime'{0}'", value);
                    break;
                default:
                    formattedValue = string.Format(CurrentCulture, "'{0}'", value.Replace("'", "''"));
                    break;
            }

            return string.Format(CurrentCulture, "{0} {1} {2}", propName, operation, formattedValue);
        }

        public static string CombineFilters(string filterA, BooleanOperator boolOperator, string filterB) 
            => string.Format(CurrentCulture, "({0}) {1} ({2})", filterA, boolOperator, filterB);
    }
}
