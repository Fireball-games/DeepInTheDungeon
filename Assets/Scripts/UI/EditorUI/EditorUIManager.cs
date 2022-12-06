using Lean.Localization;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : SingletonNotPersisting<EditorUIManager>
    {
        [SerializeField] private GameObject body;
        [SerializeField] private FileOperations fileOperations;
        [SerializeField] private NewMapDialog newMapDialog;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private TitleController mapTitle;
        [SerializeField] private RectTransform uiPoolParent;

        public StatusBar StatusBar => statusBar;
        public NewMapDialog NewMapDialog => newMapDialog;
        public static string DefaultMapName => $"< {LeanLocalization.GetTranslationText(LocalizationKeys.NewMap)} >";

        protected override void Awake()
        {
            base.Awake();
            
            ObjectPool.Instance.uiParent = uiPoolParent;
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapCreated += OnNewMapCreated;
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapCreated -= OnNewMapCreated;
        }

        private void OnNewMapCreated()
        {
            mapTitle.Show(GameController.Instance.CurrentMap.MapName);
        }
    }
}
