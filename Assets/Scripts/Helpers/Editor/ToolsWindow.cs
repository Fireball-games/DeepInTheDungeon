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
            GUILayout.Label("Map fixes:", EditorStyles.boldLabel);
            
            
            if (!Application.isPlaying)
            {
                GUILayout.Label("These tools are available in play mode.");
                return;
            }
            
            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor && MapEditorManager.Instance.MapIsPresented)
            {
                GUILayout.BeginVertical();
                
                RegenerateGuidsButton();
                SetAllSpawnPrefabOnBuildToTrueButton();
            
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("Load the map in Editor to see available tools.");
            }
            
        }

        private void RegenerateGuidsButton()
        {
            if (GUILayout.Button("Add missing GUIDs"))
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
        }

        private void SetAllSpawnPrefabOnBuildToTrueButton()
        {
            if (GUILayout.Button("Set SpawnPrefabOnBuild to true for all"))
            {
                foreach (PrefabConfiguration configuration in GameManager.Instance.CurrentMap.PrefabConfigurations)
                {
                    configuration.SpawnPrefabOnBuild = true;
                }

                MapEditorManager.Instance.SaveMap();
            }
        }
    }
}