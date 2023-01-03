using System;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.MapEditor;
using Scripts.System;
using UnityEditor;
using UnityEngine;

namespace Helpers.Editor
{
    public class ToolsWindow : EditorWindow
    {
        [MenuItem("Window/Tools Window")]
        static void Init()
        {
            ToolsWindow window = GetWindow<ToolsWindow>("Tools Window");
            
            window.Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
            {
                GUILayout.Label("Map Fixes", EditorStyles.boldLabel);
            
                GUILayout.BeginVertical();
            
                if (GUILayout.Button("Regenerate GUIDS"))
                {
                    bool changesWereMade = false;
                    foreach (PrefabConfiguration configuration in GameManager.Instance.CurrentMap.PrefabConfigurations)
                    {
                        if (string.IsNullOrEmpty(configuration.Guid))
                        {
                            changesWereMade = true;
                            configuration.Guid = Guid.NewGuid().ToString();
                        }
                    }

                    if (changesWereMade)
                    {
                        MapEditorManager.Instance.SaveMap();
                    }
                }
            
                GUILayout.EndVertical();
            }
        }
    }
}