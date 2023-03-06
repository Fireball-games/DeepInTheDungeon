using System;
using System.Threading.Tasks;
using Assets.SimpleLocalization;
using Lean.Localization;
using UnityEngine;

namespace Scripts.System
{
    public class GameStarter : MonoBehaviour
    {
        private async void Start()
        {
            // FindObjectOfType<LeanLocalization>().gameObject.SetActive(true);
            // LeanLocalization.SetCurrentLanguageAll("English");
            SetLocalization();
            
            await Task.Delay(200);
            
            GameManager.Instance.StartMainScene(false);
        }

        private void SetLocalization()
        {
            LocalizationManager.Read();

            switch (Application.systemLanguage)
            {
                // case SystemLanguage.German:
                //     LocalizationManager.Language = "German";
                //     break;
                // case SystemLanguage.Russian:
                //     LocalizationManager.Language = "Russian";
                //     break;
                default:
                    LocalizationManager.Language = "English";
                    break;
            }

            // This way you can subscribe to localization changed event.
            LocalizationManager.LocalizationChanged += () => GameManager.Instance.OnLocalizationChanged();
        }
    }
}