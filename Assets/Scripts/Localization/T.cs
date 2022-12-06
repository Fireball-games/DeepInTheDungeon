using Lean.Localization;

namespace Scripts.Localization
{
    public static class T
    {
        public static string Get(string key) => LeanLocalization.GetTranslationText(key);
    }
}