using Assets.SimpleLocalization;
using Lean.Localization;

namespace Scripts.Localization
{
    public static class t
    {
        // public static string Get(string key) => LeanLocalization.GetTranslationText(key);
        public static string Get(string key) => LocalizationManager.Localize(key);
    }
}