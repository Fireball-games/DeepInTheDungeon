using Assets.SimpleLocalization;

namespace Scripts.Localization
{
    public static class t
    {
        public static string Get(string key) => LocalizationManager.Localize(key);
    }
}