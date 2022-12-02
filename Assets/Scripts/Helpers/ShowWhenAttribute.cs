using System;
using UnityEngine;

namespace Scripts.Helpers
{
    /// <summary>
    /// Attribute used to show or hide the Field depending on certain conditions
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowWhenAttribute : PropertyAttribute
    {
        public readonly string ConditionFieldName;
        public readonly object ComparisonValue;
        public readonly object[] ComparisonValueArray;

        /// <summary>
        /// Attribute used to show or hide the Field depending on certain conditions
        /// </summary>
        /// <param name="conditionFieldName">Name of the bool condition Field</param>
        public ShowWhenAttribute(string conditionFieldName)
        {
            this.ConditionFieldName = conditionFieldName;
        }

        /// <summary>
        /// Attribute used to show or hide the Field depending on certain conditions
        /// </summary>
        /// <param name="conditionFieldName">Name of the Field to compare (bool, enum, int or float)</param>
        /// <param name="comparisonValue">Value to compare</param>
        public ShowWhenAttribute(string conditionFieldName, object comparisonValue = null)
        {
            this.ConditionFieldName = conditionFieldName;
            this.ComparisonValue = comparisonValue;
        }

        /// <summary>
        /// Attribute used to show or hide the Field depending on certain conditions
        /// </summary>
        /// <param name="conditionFieldName">Name of the Field to compare (bool, enum, int or float)</param>
        /// <param name="comparisonValueArray">Array of values to compare (only for enums)</param>
        public ShowWhenAttribute(string conditionFieldName, object[] comparisonValueArray = null)
        {
            this.ConditionFieldName = conditionFieldName;
            this.ComparisonValueArray = comparisonValueArray;
        }
    }
}