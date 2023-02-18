using System;
using System.IO;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.System;
using UnityEditor;
using UnityEngine;
using static Scripts.Helpers.FileOperationsHelper;
using Logger = Scripts.Helpers.Logger;

namespace Helpers.Editor
{
    public class ToolsWindow : EditorWindow
    {
        private static GUIStyle _deselectedStyle;
        private static GUIStyle _warningStyle;
        private bool _initialized;

        [MenuItem("Window/Tools Window")]
        static void Init()
        {
            ToolsWindow window = GetWindow<ToolsWindow>("Tools Window");

            window.Show();
        }
        
        private void Initialize()
        {
            _deselectedStyle = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    textColor = Color.black
                }
            };
            
            _warningStyle = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    textColor = Color.red
                }
            };

            _initialized = true;
        }

        private void OnGUI()
        {
            if (!_initialized) Initialize();
            
            PlayModeTools();

            CopyMainCampaignToResourcesButton();
            CopyStartRoomsToResourcesButton();
            CopyDemoToResourcesButton();
            
            CopyAllResourcesCampaignsToLocalLowButton();
        }

        private void PlayModeTools()
        {
            GUILayout.Label("Map fixes:", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                GUILayout.Label("These tools are available in play mode.");
                return;
            }

            if (!GameManager.Instance) return;

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

            GUILayout.Space(20);
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

        private void CopyMainCampaignToResourcesButton()
        {
            if (GUILayout.Button("Copy Main Campaign to Resources"))
            {
                CopyCampaignToResources(Strings.MainCampaignName);
            }
        }

        private void CopyStartRoomsToResourcesButton()
        {
            if (GUILayout.Button("Copy Start Rooms to Resources"))
            {
                CopyCampaignToResources(Strings.StartRoomsCampaignName);
            }
        }
        
        private void CopyDemoToResourcesButton()
        {
            if (GUILayout.Button("Copy Start Rooms to Resources"))
            {
                CopyCampaignToResources(Strings.DemoCampaignName);
            }
        }

        private void CopyCampaignToResources(string campaignName)
        {
            try
            {
                string sourcePath = GetFullCampaignPath(campaignName);
                string destinationPath = Path.Combine(FullCampaignsResourcesPath, $"{campaignName}{CampaignFileExtension}");

                File.Copy(sourcePath, destinationPath, true);
                AssetDatabase.Refresh();
                
                GetWindow<SceneView>().ShowNotification(new GUIContent($"Campaign {campaignName} copied to Resources."));
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
        
        private void CopyAllResourcesCampaignsToLocalLowButton()
        {
            if (GUILayout.Button("Copy all Resources campaigns to LocalLow", _warningStyle))
            {
                try
                {
                    string[] files = Directory.GetFiles(FullCampaignsResourcesPath, $"*{CampaignFileExtension}");

                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(CampaignsLocalDirectoryPath, fileName);

                        File.Copy(file, destinationPath, true);
                    }

                    AssetDatabase.Refresh();
                    
                    GetWindow<SceneView>().ShowNotification(new GUIContent("All Resources campaigns copied to LocalLow."));
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            }
        }
    }
}