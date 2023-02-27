using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using UnityEngine;

namespace Scripts.Helpers
{
    /// <summary>
    ///  If consumer of this class finds PrefabName in the map in this list, he will get Vector3 saying
    /// what scale this prefab should use in the editor. 
    /// </summary>
    public static class EditorPrefabScaleOverrides
    {
        private static readonly Dictionary<string, Vector3> ScaleMap = new()
        {
            {"DissolveWall", 1.05f.ToVectorUniform()},
        };

        public static bool Get(string prefabName, out Vector3 overrideScale) =>
            ScaleMap.TryGetValue(prefabName, out overrideScale);
    }
}