using System.Collections;
using Scripts.EventsManagement;
using Scripts.System;
using UnityEngine;
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
        
        public void LoadMainScene()
        {
            StartCoroutine(LoadSceneAsync(Scenes.MainSceneName));
        }

        public void LoadEditorScene()
        {
            StartCoroutine(LoadSceneAsync(Scenes.EditorSceneName));
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
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
