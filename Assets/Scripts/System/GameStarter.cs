using System.Threading.Tasks;
using Lean.Localization;
using UnityEngine;

namespace Scripts.System
{
    public class GameStarter : MonoBehaviour
    {
        private async void Start()
        {
            FindObjectOfType<LeanLocalization>().gameObject.SetActive(true);
            LeanLocalization.SetCurrentLanguageAll("English");
            
            await Task.Delay(200);
            
            GameManager.Instance.StartMainScene(false);
        }
    }
}