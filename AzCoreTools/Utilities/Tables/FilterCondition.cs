using CoreTools.Utilities;
using System;

namespace AzCoreTools.Utilities.Tables
{
    public struct FilterCondition
    {
        private readonly string condition;

        private FilterCondition(string condition)
        {
            this.condition = condition;
        }

        public FilterCondition(string propName, QueryComparison operation, string value)
        {
            condition = TableQueryBuilder.GenerateFilterCondition(propName, operation, value);
        }

        public FilterCondition(string propName, QueryComparison operation, DateTime value)
        {
            condition = TableQueryBuilder.GenerateFilterCondition(propName, operation, value);
        }

        public FilterCondition And(FilterCondition filterConditionB)
        {
            return new FilterCondition(TableQueryBuilder.CombineFilters(condition, BooleanOperator.and, filterConditionB.condition));
        }

        public FilterCondition Or(FilterCondition filterConditionB)
        {
            return new FilterCondition(TableQueryBuilder.CombineFilters(condition, BooleanOperator.or, filterConditionB.condition));
        }

        public override string ToString()
        {
            return condition;
        }
    }
}
