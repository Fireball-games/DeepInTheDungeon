using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts.Helpers;
using UnityEditor;
using UnityEngine;

namespace Helpers.Editor
{
    [CustomPropertyDrawer(typeof(ShowWhenAttribute))]
    public class ShowWhenPropertyDrawer : PropertyDrawer
    {
        private bool _showField = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowWhenAttribute showWhenAttribute = (ShowWhenAttribute) attribute;
            SerializedProperty conditionField = property.serializedObject.FindProperty(showWhenAttribute.ConditionFieldName);

            // We check that exist a Field with the parameter name
            if (conditionField == null)
            {
                ShowError(position, label, "Error getting the condition Field. Check the name.");
                return;
            }

            switch (conditionField.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    try
                    {
                        bool comparisonValue = showWhenAttribute.ComparisonValue == null || (bool) showWhenAttribute.ComparisonValue;
                        _showField = conditionField.boolValue == comparisonValue;
                    }
                    catch
                    {
                        ShowError(position, label, "Invalid comparison Value Type");
                        return;
                    }

                    break;
                case SerializedPropertyType.Enum:
                    object paramEnum = showWhenAttribute.ComparisonValue;
                    object[] paramEnumArray = showWhenAttribute.ComparisonValueArray;

                    if (paramEnum == null && paramEnumArray == null)
                    {
                        ShowError(position, label, "The comparison enum value is null");
                        return;
                    }

                    if (IsEnum(paramEnum))
                    {
                        if (!CheckSameEnumType(new[] {paramEnum!.GetType()},
                                property.serializedObject.targetObject.GetType(), conditionField.propertyPath))
                        {
                            ShowError(position, label, "Enum Types doesn't match");
                            return;
                        }

                        string enumValue = Enum.GetValues(paramEnum.GetType()).GetValue(conditionField.enumValueIndex)
                            .ToString();
                        _showField = paramEnum.ToString() == enumValue;
                    }
                    else if (IsEnum(paramEnumArray))
                    {
                        if (!CheckSameEnumType(paramEnumArray.Select(x => x.GetType()),
                                property.serializedObject.targetObject.GetType(), conditionField.propertyPath))
                        {
                            ShowError(position, label, "Enum Types doesn't match");
                            return;
                        }

                        string enumValue = Enum.GetValues(paramEnumArray[0].GetType())
                            .GetValue(conditionField.enumValueIndex).ToString();
                        _showField = paramEnumArray.Any(x => x.ToString() == enumValue);
                    }
                    else
                    {
                        ShowError(position, label, "The comparison enum value is not an enum");
                        return;
                    }

                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    string stringValue;
                    bool error = false;

                    float conditionValue = conditionField.propertyType switch
                    {
                        SerializedPropertyType.Integer => conditionField.intValue,
                        SerializedPropertyType.Float => conditionField.floatValue,
                        _ => 0
                    };

                    try
                    {
                        stringValue = (string) showWhenAttribute.ComparisonValue;
                    }
                    catch
                    {
                        ShowError(position, label, "Invalid comparison Value Type");
                        return;
                    }

                    if (stringValue.StartsWith("=="))
                    {
                        float? value = GetValue(stringValue, "==");
                        if (value == null)
                            error = true;
                        else
                            _showField = Equals(conditionValue, value);
                    }
                    else if (stringValue.StartsWith("!="))
                    {
                        float? value = GetValue(stringValue, "!=");
                        if (value == null)
                            error = true;
                        else
                            _showField = !Equals(conditionValue, value);
                    }
                    else if (stringValue.StartsWith("<="))
                    {
                        float? value = GetValue(stringValue, "<=");
                        if (value == null)
                            error = true;
                        else
                            _showField = conditionValue <= value;
                    }
                    else if (stringValue.StartsWith(">="))
                    {
                        float? value = GetValue(stringValue, ">=");
                        if (value == null)
                            error = true;
                        else
                            _showField = conditionValue >= value;
                    }
                    else if (stringValue.StartsWith("<"))
                    {
                        float? value = GetValue(stringValue, "<");
                        if (value == null)
                            error = true;
                        else
                            _showField = conditionValue < value;
                    }
                    else if (stringValue.StartsWith(">"))
                    {
                        float? value = GetValue(stringValue, ">");
                        if (value == null)
                            error = true;
                        else
                            _showField = conditionValue > value;
                    }

                    if (error)
                    {
                        ShowError(position, label, "Invalid comparison instruction for Int or float value");
                        return;
                    }

                    break;
                default:
                    ShowError(position, label, "This type has not supported.");
                    return;
            }

            if (_showField)
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_showField)
                return EditorGUI.GetPropertyHeight(property);
            else
                return -EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        /// Return if the object is enum and not null
        /// </summary>
        private static bool IsEnum(object obj)
        {
            return obj != null && obj.GetType().IsEnum;
        }

        /// <summary>
        /// Return if all the objects are enums and not null
        /// </summary>
        private static bool IsEnum(object[] obj)
        {
            return obj != null && obj.All(o => o.GetType().IsEnum);
        }

        /// <summary>
        /// Check if the field with name "fieldName" has the same class as the "checkTypes" classes through reflection
        /// </summary>
        private static bool CheckSameEnumType(IEnumerable<Type> checkTypes, Type classType, string fieldName)
        {
            FieldInfo memberInfo;
            string[] fields = fieldName.Split('.');
            if (fields.Length > 1)
            {
                memberInfo = classType.GetField(fields[0]);
                for (int i = 1; i < fields.Length; i++)
                {
                    memberInfo = memberInfo.FieldType.GetField(fields[i]);
                }
            }
            else
                memberInfo = classType.GetField(fieldName);

            return memberInfo != null && checkTypes.All(x => x == memberInfo.FieldType);
        }

        private void ShowError(Rect position, GUIContent label, string errorText)
        {
            EditorGUI.LabelField(position, label, new GUIContent(errorText));
            _showField = true;
        }

        /// <summary>
        /// Return the float value in the content string removing the remove string
        /// </summary>
        private static float? GetValue(string content, string remove)
        {
            string removed = content.Replace(remove, "");
            try
            {
                return float.Parse(removed);
            }
            catch
            {
                return null;
            }
        }
    }
}