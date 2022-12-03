using System;
using System.Collections;
using Scripts.EventsManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Scripts.Helpers.Logger;

namespace Scripts
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private void OnEnable()
        {
            EventsManager.OnOpenEditorRequested += LoadEditorScene;
        }

        private void OnDisable()
        {
            EventsManager.OnOpenEditorRequested -= LoadEditorScene;
        }

        private void LoadEditorScene()
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
            
            EventsManager.TriggerOnSceneFinishedLoading(sceneName);            
        }
    }
}
