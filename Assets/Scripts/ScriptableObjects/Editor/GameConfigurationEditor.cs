using UnityEditor;
using UnityEngine;
using static Scripts.Enums;

namespace Scripts.ScriptableObjects.Editor
{
    [CustomEditor(typeof(GameConfiguration))]
    public class GameConfigurationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameConfiguration config = (GameConfiguration)target;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Selected Main Campaign: ", GUILayout.Width(150));
            
            GUIStyle selectedStyle = new(GUI.skin.button)
            {
                normal =
                {
                    textColor = Color.green
                }
            };
            
            GUIStyle deselectedStyle = new(GUI.skin.button)
            {
                normal =
                {
                    textColor = Color.black
                }
            };

            if (GUILayout.Button("Main Campaign", config.selectedMainCampaign == EMainCampaignName.MainCampaign ? selectedStyle : deselectedStyle))
            {
                config.selectedMainCampaign = EMainCampaignName.MainCampaign;
            }

            if (GUILayout.Button("Demo", config.selectedMainCampaign == EMainCampaignName.Demo ? selectedStyle : deselectedStyle))
            {
                config.selectedMainCampaign = EMainCampaignName.Demo;
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}