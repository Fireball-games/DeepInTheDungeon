using System.Linq;

namespace Scripts.ScenesManagement
{
    public static class Scenes
    {
        public const string EditorSceneName = "Editor";
        public const string MainSceneName = "Main";
        public const string PlayIndoorSceneName = "PlayIndoor";
        public const string PlayOutdoorSceneName = "PlayOutdoor";

        private static readonly string[] SceneNames =
        {
            EditorSceneName,
            MainSceneName,
            PlayIndoorSceneName,
            PlayOutdoorSceneName
        };

        public static bool IsValidSceneName(string sceneName) => SceneNames.Contains(sceneName);
    }
}