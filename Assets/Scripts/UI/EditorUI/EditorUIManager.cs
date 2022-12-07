using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : SingletonNotPersisting<EditorUIManager>
    {
        [SerializeField] private ImageButton playButton;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private DialogBase confirmationDialog;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private TitleController mapTitle;
        [SerializeField] private RectTransform uiPoolParent;
        [SerializeField] private MapEditorManager manager;

        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public DialogBase ConfirmationDialog => confirmationDialog;

        private void OnEnable()
        {
            ObjectPool.Instance.uiParent = uiPoolParent;
            
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
            mapTitle.Show(GameManager.Instance.CurrentMap.MapName);
        }
    }
}
