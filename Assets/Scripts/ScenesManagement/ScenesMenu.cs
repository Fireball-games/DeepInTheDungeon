using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Scripts.ScenesManagement
{
    public static class ScenesMenu
    {
#if UNITY_EDITOR
        private const string ScenesDirectory = "Assets/Scenes";

        [MenuItem("Scenes/Main Scene", false)]
        private static void ShowMainScene() => LoadScene(Scenes.MainSceneName);
        
        [MenuItem("Scenes/Editor Scene", false)]
        private static void ShowEditorScene() => LoadScene(Scenes.EditorSceneName);
        
        [MenuItem("Scenes/PlayIndoor Scene", false)]
        private static void ShowPlayIndoorScene() => LoadScene(Scenes.PlayIndoorSceneName);

        private static void LoadScene(string sceneName)
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();

            if (currentScene.name == sceneName || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(BuildScenePath(sceneName));
            EditorSceneManager.CloseScene(currentScene, true);
        }

        private static string BuildScenePath(string sceneName) => $"{ScenesDirectory}/{sceneName}.unity";
#endif
    }
}