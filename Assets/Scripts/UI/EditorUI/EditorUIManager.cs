using Lean.Localization;
using Scripts.EventsManagement;
using Scripts.Localization;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class EditorUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private FileOperations fileOperations;
        [SerializeField] private StatusBar statusBar;
        [SerializeField] private TitleController mapTitle;
        [SerializeField] private RectTransform uiPoolParent;

        public static StatusBar StatusBar;
        public static string DefaultMapName => $"< {LeanLocalization.GetTranslationText(LocalizationKeys.NewMap)} >";
        
        private void Awake()
        {
            StatusBar = statusBar ??= FindObjectOfType<StatusBar>();
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
            mapTitle.Show(DefaultMapName);
        }
    }
}
