using Assets.SimpleLocalization;
using Scripts.Player.CharacterSystem;

namespace Scripts.Localization
{
    public static class t
    {
        private const string ItemPrefix = "Item/";
        private const string ItemDetailPrefix = "ItemDetail/";
        private const string ItemDefault = "Item";
        private const string ItemDetailDefault = "Item";
        private const string StatPrefix = "Stat/";
        private const string StatDefault = "Stat";
        private const string TooltipPrefix = "Tooltip/";
        private const string TooltipDefault = "Tooltip is missing";
        
        public static string Get(string key) => LocalizationManager.Localize(key);
        
        public static string GetTooltipText(string key) => GetPrefixedKey(TooltipPrefix, TooltipDefault, key);
        public static string GetItemText(string key) => GetPrefixedKey(ItemPrefix, ItemDefault, key);
        public static string GetItemDetailText(string key) => GetPrefixedKey(ItemDetailPrefix, ItemDetailDefault, key);
        public static string GetStatPrefix(CharacterProfile.ECharacterStats modifierStat) =>
            GetPrefixedKey(StatPrefix, StatDefault, modifierStat.ToString());
        
        private static string GetPrefixedKey(string prefix, string defaultText, string key)
        {
            string text = Get($"{prefix}{key}");
            return string.IsNullOrEmpty(text) ? defaultText : text;
        }

    }
}