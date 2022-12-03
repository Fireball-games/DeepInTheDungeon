using Scripts;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : Singleton<MainUIManager>
{
    public GameObject ButtonsWrapper;
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

    private void OnLevelStarted() => ButtonsWrapper.SetActive(false);

    private void OnSceneFinishedLoading(string sceneName)
    {
        if (sceneName == Scenes.EditorSceneName)
            OnLevelStarted();
    }
}