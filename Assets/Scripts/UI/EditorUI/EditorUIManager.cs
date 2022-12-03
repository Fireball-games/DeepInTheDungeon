using System;
using Lean.Localization;
using Scripts.EventsManagement;
using Scripts.Localization;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private FileOperations fileOperations;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private TitleController mapTitle;

        public static StatusBar StatusBar;
        
        private void Awake()
        {
            StatusBar = statusBar ??= FindObjectOfType<StatusBar>();
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
            mapTitle.Show($"< {LeanLocalization.GetTranslationText(LocalizationKeys.NewMap)} >");
        }
    }
}
