using System.Collections;
using System.Threading.Tasks;
using Scripts.EventsManagement;
using Scripts.System.MonoBases;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.ScenesManagement
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private static string _currentSceneName;
        
        public static bool IsInMainScene => _currentSceneName == Scenes.MainSceneName;
        
        public async void LoadScene(string sceneName, bool fadeIn)
        {
            await LoadSceneAsync(sceneName, fadeIn);
        }

        public async void LoadEditorScene()
        {
           await LoadSceneAsync(Scenes.EditorSceneName);
        }
        
        private static async Task<bool> LoadSceneAsync(string sceneName, bool fadeIn = true, UnityAction onLoadFinished = null)
        {
            if (fadeIn)
            {
                await ScreenFader.FadeIn(0.5f);
            }

            if (!Scenes.IsValidSceneName(sceneName))
            {
                Logger.LogWarning("Invalid scene name");
                return false;
            }

            EventsManager.TriggerOnSceneStartedLoading();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }

            _currentSceneName = sceneName;
            EventsManager.TriggerOnSceneFinishedLoading(sceneName);
            onLoadFinished?.Invoke();
            return true;
        }
    }
}
