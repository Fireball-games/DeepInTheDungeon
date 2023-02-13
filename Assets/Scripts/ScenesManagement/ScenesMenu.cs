using UnityEditor;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
    using UnityEditor.SceneManagement;
#endif

namespace Scripts.ScenesManagement
{
    public static class ScenesMenu
    {
#if UNITY_EDITOR
        private const string ScenesDirectory = "Assets/Scenes";
        
        [MenuItem("Scenes/Starter Scene", false)]
        private static void ShowStarterScene() => LoadScene(Scenes.StartSceneName);

        [MenuItem("Scenes/Main Scene", false)]
        private static void ShowMainScene() => LoadScene(Scenes.MainSceneName);
        
        [MenuItem("Scenes/Editor Scene", false)]
        private static void ShowEditorScene() => LoadScene(Scenes.EditorSceneName);
        
        [MenuItem("Scenes/PlayIndoor Scene", false)]
        private static void ShowPlayIndoorScene() => LoadScene(Scenes.PlayIndoorSceneName);
        
        [MenuItem("Scenes/Sandbox Scene", false)]
        private static void ShowSandboxScene() => LoadScene(Scenes.SandboxSceneName);

        private static void LoadScene(string sceneName)
        {
#if UNITY_EDITOR
            Scene currentScene = EditorSceneManager.GetActiveScene();

            if (currentScene.name == sceneName || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(BuildScenePath(sceneName));
            EditorSceneManager.CloseScene(currentScene, true);
#else
            SceneManager.LoadScene(sceneName);
#endif
        }

        private static string BuildScenePath(string sceneName) => $"{ScenesDirectory}/{sceneName}.unity";
#endif
    }
}