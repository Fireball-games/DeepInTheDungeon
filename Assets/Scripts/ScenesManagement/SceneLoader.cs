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
        
        public async void LoadScene(string sceneName, bool fadeIn, float fadeInDuration = 0.5f, UnityAction onLoadFinished = null) 
            => await LoadSceneAsync(sceneName, fadeIn, fadeInDuration, onLoadFinished);

        public async void LoadEditorScene()
        {
           await LoadSceneAsync(Scenes.EditorSceneName);
        }
        
        // ReSharper disable once UnusedMethodReturnValue.Local -> it's intentional
        private static async Task<bool> LoadSceneAsync(string sceneName, bool fadeIn = true, float fadeInDuration = 0.5f, UnityAction onLoadFinished = null)
        {
            if (fadeIn)
            {
                await ScreenFader.FadeIn(fadeInDuration);
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
