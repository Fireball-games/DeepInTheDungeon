using Scripts.EventsManagement;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class MainUIManager : Singleton<MainUIManager>
    {
        public GameObject body;
        public GameObject HUD;
        public Button PlayButton;
        public Button EditorButton;
        [SerializeField] private RectTransform poolStore;

        protected override void Awake()
        {
            base.Awake();

            ObjectPool.Instance.uiParent = poolStore;
        }

        private void OnEnable()
        {
            PlayButton.onClick.AddListener(OnPlayButtonClick);
            EditorButton.onClick.AddListener(OnEditorButtonClick);

            EventsManager.OnLevelStarted += OnLevelStarted;
            EventsManager.OnSceneFinishedLoading += OnSceneFinishedLoading;
        }

        private void OnDisable()
        {
            PlayButton.onClick.RemoveListener(OnPlayButtonClick);
            EditorButton.onClick.RemoveListener(OnEditorButtonClick);
        
            EventsManager.OnLevelStarted -= OnLevelStarted;
            EventsManager.OnSceneFinishedLoading -= OnSceneFinishedLoading;
        }

        private void OnPlayButtonClick()
        {
            EventsManager.TriggerOnStartGameRequested();
        }

        private void OnEditorButtonClick() => SceneLoader.Instance.LoadEditorScene();

        private void OnLevelStarted()
        {
            HUD ??= FindObjectOfType<HUDController>(true).gameObject;
            
            if (HUD)
            {
                body.SetActive(false);
                HUD.SetActive(true);
            }
        }

        private void OnSceneFinishedLoading(string sceneName)
        {
            body.SetActive(sceneName == Scenes.MainSceneName);
        }
    }
}
