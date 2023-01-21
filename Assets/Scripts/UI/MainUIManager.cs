using Scripts.EventsManagement;
using Scripts.ScenesManagement;
using Scripts.System.MonoBases;
using Scripts.UI.PlayMode;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class MainUIManager : SingletonNotPersisting<MainUIManager>
    {
        public GameObject body;
        public Button PlayButton;
        public Button EditorButton;

        private GameObject _hud;

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
            _hud ??= FindObjectOfType<HUDController>(true).gameObject;
            
            if (_hud)
            {
                body.SetActive(false);
                _hud.SetActive(true);
            }
        }

        private void OnSceneFinishedLoading(string sceneName)
        {
            body.SetActive(sceneName == Scenes.MainSceneName);
        }
    }
}
