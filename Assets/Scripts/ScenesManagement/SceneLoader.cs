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
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        public async Task LoadMainMenuScene(bool fadeIn = true, UnityAction onSceneLoaded = null)
        {
            await LoadSceneAsync(Scenes.MainSceneName, fadeIn, onSceneLoaded);
        }

        public void LoadMainScene()
        {
            StartCoroutine(LoadSceneAsync(Scenes.MainSceneName));
        }

        public void LoadEditorScene()
        {
            StartCoroutine(LoadSceneAsync(Scenes.EditorSceneName));
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

            EventsManager.TriggerOnSceneFinishedLoading(sceneName);
            onLoadFinished?.Invoke();
            return true;
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            EventsManager.TriggerOnSceneStartedLoading();
            
            if (!Scenes.IsValidSceneName(sceneName))
            {
                Logger.LogWarning("Invalid scene name");
                yield break;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return null;
            
            EventsManager.TriggerOnSceneFinishedLoading(sceneName);            
        }
    }
}
