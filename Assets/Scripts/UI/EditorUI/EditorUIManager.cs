using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : SingletonNotPersisting<EditorUIManager>
    {
        [SerializeField] private ImageButton playButton;
        [SerializeField] private WorkModeSelection workModeSelection;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private DialogBase confirmationDialog;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private Title mapTitle;
        [SerializeField] private MapEditorManager manager;

        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public DialogBase ConfirmationDialog => confirmationDialog;

        private void OnEnable()
        {
            playButton.OnClick += manager.PlayMap;
            EditorEvents.OnNewMapCreated += OnNewMapCreated;
        }

        private void OnDisable()
        {
            playButton.OnClick -= manager.PlayMap;
            EditorEvents.OnNewMapCreated -= OnNewMapCreated;
        }

        private void OnNewMapCreated()
        {
            playButton.SetActive(true);
            workModeSelection.SetActive(true);
            mapTitle.Show(GameManager.Instance.CurrentMap.MapName);
        }
    }
}
