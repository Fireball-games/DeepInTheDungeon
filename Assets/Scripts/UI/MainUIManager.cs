using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class MainUIManager : Singleton<MainUIManager>
    {
        public GameObject ButtonsWrapper;
        public GameObject HUD;
        public Button PlayButton;
        public Button EditorButton;

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

        private void OnEditorButtonClick() => EventsManager.TriggerOnOpenEditorRequested();

        private void OnLevelStarted()
        {
            HUD.SetActive(true);
            ButtonsWrapper.SetActive(false);
        }

        private void OnSceneFinishedLoading(string sceneName)
        {
            if (sceneName == Scenes.EditorSceneName)
            {
                OnLevelStarted();
            }
        }
    }
}
