using System.Linq;
using UnityEditor.U2D;

namespace Scripts
{
    public static class Scenes
    {
        public const string EditorSceneName = "Editor";

        private static readonly string[] SceneNames = { EditorSceneName };

        public static bool IsValidSceneName(string sceneName) => SceneNames.Contains(sceneName);
    }
}