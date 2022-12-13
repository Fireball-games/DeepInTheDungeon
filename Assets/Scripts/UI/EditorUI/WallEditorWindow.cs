using Scripts.Building.Walls;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.MapEditor.Enums;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI
{
    public class WallEditorWindow : EditorWindowBase
    {
        [SerializeField] private PrefabList prefabList;
        [SerializeField] private Button CancelButton;
        [SerializeField] private GameObject placeholderWall;

        private IPrefab _selectedPrefab = null;

        private void Awake()
        {
            CancelButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += Close;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= Close;
        }

        public void Open(EWallType wallType, Vector3 placeholderPosition)
        {
            SetActive(true);

            CancelButton.GetComponentInChildren<TMP_Text>().text = T.Get(LocalizationKeys.Cancel);
            string title = T.Get(LocalizationKeys.AvailablePrefabs);

            placeholderWall.transform.position = placeholderPosition;
            placeholderWall.SetActive(true);

            prefabList.Open(title, new []{ "Prefab 1", "Prefab 2"}, SetPrefab);
        }

        private void SetPrefab(string prefabName)
        {
            Logger.Log($"Selected item: {prefabName}");
        }

        private void Close(EWorkMode _) => Close();
        
        public void Close()
        {
            placeholderWall.transform.position = Vector3.zero;
            placeholderWall.transform.parent = body.transform;
            placeholderWall.SetActive(false);
            EditorUIManager.Instance.IsAnyObjectEdited = false;
            
            SetActive(false);
        }
    }
}