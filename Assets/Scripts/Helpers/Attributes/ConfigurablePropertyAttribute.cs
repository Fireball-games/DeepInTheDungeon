using System;

namespace Scripts.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigurablePropertyAttribute : Attribute
    {
        /// <summary>
        /// This field is used to set value in configuration.
        /// </summary>
        public readonly string ConfigurationFieldName;
        /// <summary>
        /// Label text key for component in editor. Label on component is set through this key,
        /// since it is not possible to set it directly, because t.Get method is not constant.
        /// </summary>
        public readonly string LabelText;
        /// <summary>
        /// Method in editor class that is used on change of value in editor.
        /// </summary>
        public readonly string ConfigurationPropertySetterMethod;
        /// <summary>
        /// This field is available also for embedded prefabs.
        /// </summary>
        public readonly bool IsAvailableForEmbedded;
        /// <summary>
        /// Some Components are too complex to set in general manner, in that case, component is assumed to be present and used.
        /// </summary>
        public readonly bool UseLocalPrefabInstance;
        /// <summary>
        /// Some components dont need value to work, you set this fact with this bool.
        /// </summary>
        public readonly bool SetValueFromConfiguration;
        
        public ConfigurablePropertyAttribute(
            string configurationFieldName,
            string labelText,
            string configurationPropertySetterMethod,
            bool isAvailableForEmbedded = true,
            bool useLocalPrefabInstance = false,
            bool setValueFromConfiguration = true)
        {
            ConfigurationFieldName = configurationFieldName;
            LabelText = labelText;
            ConfigurationPropertySetterMethod = configurationPropertySetterMethod;
            IsAvailableForEmbedded = isAvailableForEmbedded;
            UseLocalPrefabInstance = useLocalPrefabInstance;
            SetValueFromConfiguration = setValueFromConfiguration;
        }
        
    }
}