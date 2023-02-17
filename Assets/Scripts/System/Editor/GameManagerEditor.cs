using Scripts.ScriptableObjects;
using UnityEditor;

namespace Scripts.System.Editor
{
    // [CustomEditor(typeof(GameManager))]
    // public class GameManagerEditor : UnityEditor.Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         serializedObject.Update();
    //     
    //         SerializedProperty gameConfigurationProperty = serializedObject.FindProperty("gameConfiguration");
    //         bool isGameConfigurationAssigned = gameConfigurationProperty.objectReferenceValue != null;
    //         
    //         GameConfiguration gameConfiguration = (GameConfiguration)gameConfigurationProperty.objectReferenceValue;
    //         SerializedObject gameConfigurationSerializedObject = new(gameConfiguration);
    //
    //         if (isGameConfigurationAssigned)
    //         {
    //             EditorGUILayout.LabelField("Game Configuration properties", EditorStyles.boldLabel);
    //
    //             EditorGUI.BeginDisabledGroup(true);
    //             EditorGUILayout.PropertyField(gameConfigurationSerializedObject.FindProperty("selectedMainCampaign"));
    //             EditorGUI.EndDisabledGroup();
    //         }
    //
    //         EditorGUILayout.PropertyField(serializedObject.FindProperty("mapBuilder"));
    //         EditorGUILayout.PropertyField(serializedObject.FindProperty("playerPrefab"));
    //
    //         serializedObject.ApplyModifiedProperties();
    //     }
    // }
}