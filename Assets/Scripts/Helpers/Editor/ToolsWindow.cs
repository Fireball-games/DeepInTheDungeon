using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Scripts.Helpers.FileOperationsHelper;
using static Scripts.Helpers.Strings;
using Logger = Scripts.Helpers.Logger;

namespace Helpers.Editor
{
    public class ToolsWindow : EditorWindow
    {
        private static GUIStyle _deselectedStyle;
        private static GUIStyle _warningStyle;

        private string[] fontNames;
        int selectedFontIndex = 0;

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
                    textColor = Color.gray
                }
            };

            _warningStyle = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    textColor = Color.yellow
                }
            };
        }

        private void OnGUI()
        {
            if (_deselectedStyle == null) Initialize();

            PlayModeTools();
            EditorGUILayout.Separator();
            FileOperationsTools();
            EditorGUILayout.Separator();
            MiscellaneousTools();
        }

        private void PlayModeTools()
        {
            GUILayout.Label("Map fixes:", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                GUILayout.Label("These tools are available in play mode.");
                return;
            }

            if (!GameManager.Instance || !MapEditorManager.Instance) return;

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

        private void FileOperationsTools()
        {
            GUILayout.Label("File operations:", EditorStyles.boldLabel);

            GUILayout.BeginVertical();

            CopyMainCampaignToResourcesButton();
            CopyStartRoomsToResourcesButton();
            CopyDemoToResourcesButton();
            EditorGUILayout.Space(10);
            CopyAllResourcesCampaignsToLocalLowButton();
            EditorGUILayout.Separator();
            OpenLocalLowFolderButton();
            EditorGUILayout.Space(10);
            EditorGUILayout.Separator();
            GUILayout.EndVertical();
        }

        private void MiscellaneousTools()
        {
            GUILayout.Label("Miscellaneous:", EditorStyles.boldLabel);

            GUILayout.BeginVertical();

            SwapFonts();

            GUILayout.EndVertical();
        }

        private void OpenLocalLowFolderButton()
        {
            if (GUILayout.Button("Open LocalLow folder"))
            {
                OpenFolder(Path.Combine(Application.persistentDataPath));
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

        private void CopyMainCampaignToResourcesButton()
        {
            if (GUILayout.Button("Copy Main Campaign to Resources"))
            {
                CopyCampaignToResources(MainCampaignName);
            }
        }

        private void CopyStartRoomsToResourcesButton()
        {
            if (GUILayout.Button("Copy Start Rooms to Resources"))
            {
                CopyCampaignToResources(StartRoomsCampaignName);
            }
        }

        private void CopyDemoToResourcesButton()
        {
            if (GUILayout.Button("Copy Demo Campaign to Resources"))
            {
                CopyCampaignToResources(DemoCampaignName);
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

        private void SwapFonts()
        {
            fontNames = GetFontNames();

            selectedFontIndex = EditorGUILayout.Popup("Select Font", selectedFontIndex, fontNames);

            if (GUILayout.Button("Swap Font"))
            {
                SwapFont(selectedFontIndex);
            }
        }

        private string[] GetFontNames() => Resources.LoadAll<TMP_FontAsset>("Fonts & Materials").Select(font => font.name).ToArray();

        private void SwapFont(int fontIndex)
        {
            string fontName = fontNames[fontIndex];
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/" + fontName);

            if (font)
            {
                foreach (TMP_Text tmpText in FindObjectsOfType<TMP_Text>())
                {
                    tmpText.font = font;
                }
            }
            else
            {
                Debug.LogError("Font " + fontName + " not found in Resources/Fonts folder");
            }
        }
        
        private string[] LoadAllPrefabs(string rootPath)
        {
            List<string> prefabPaths = new();

            foreach (string filePath in Directory.GetFiles(rootPath))
            {
                if (Path.GetExtension(filePath) == ".prefab")
                {
                    prefabPaths.Add(filePath);
                }
            }

            foreach (string directoryPath in Directory.GetDirectories(rootPath))
            {
                prefabPaths.AddRange(LoadAllPrefabs(directoryPath));
            }

            return prefabPaths.ToArray();
        }
    }
}