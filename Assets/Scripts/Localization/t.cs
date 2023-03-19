using Assets.SimpleLocalization;

namespace Scripts.Localization
{
    public static class t
    {
        private const string ItemPrefix = "Item/";
        private const string ItemDetailPrefix = "ItemDetail/";
        private const string ItemDefault = "Item";
        private const string ItemDetailDefault = "Item";
        private const string TooltipPrefix = "Tooltip/";
        private const string TooltipDefault = "Tooltip is missing";
        
        public static string Get(string key) => LocalizationManager.Localize(key);
        
        public static string GetTooltipText(string key) => GetPrefixedKey(TooltipPrefix, TooltipDefault, key);
        public static string GetItemText(string key) => GetPrefixedKey(ItemPrefix, ItemDefault, key);
        public static string GetItemDetailText(string key) => GetPrefixedKey(ItemDetailPrefix, ItemDetailDefault, key);
        
        private static string GetPrefixedKey(string prefix, string defaultText, string key)
        {
            string text = Get($"{prefix}{key}");
            return string.IsNullOrEmpty(text) ? defaultText : text;
        }
    }
}