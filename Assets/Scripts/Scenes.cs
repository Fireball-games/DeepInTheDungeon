﻿using System.Linq;
using UnityEditor.U2D;

namespace Scripts
{
    public static class Scenes
    {
        public const string EditorSceneName = "Editor";
        public const string MainSceneName = "Main";

        private static readonly string[] SceneNames =
        {
            EditorSceneName,
            MainSceneName,
        };

        public static bool IsValidSceneName(string sceneName) => SceneNames.Contains(sceneName);
    }
}