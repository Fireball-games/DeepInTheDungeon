using System;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    // ***********    Publishers    ***********
    public static event Action OnStartGameRequested;
    public static event Action OnOpenEditorRequested;
    public static event Action OnLevelStarted;
    public static event Action<string> OnSceneFinishedLoading;
    public static event Action OnModalShowRequested;
    public static event Action OnModalHideRequested;
    
    // ***********    Triggers    ***********

    public static void TriggerOnStartGameRequested() => OnStartGameRequested?.Invoke();
    public static void TriggerOnOpenEditorRequested() => OnOpenEditorRequested?.Invoke();
    public static void TriggerOnLevelStarted() => OnLevelStarted?.Invoke();
    public static void TriggerOnSceneFinishedLoading(string sceneName) => OnSceneFinishedLoading?.Invoke(sceneName);
    public static void TriggerOnModalShowRequested() => OnModalShowRequested?.Invoke();
    public static void TriggerOnModalHideRequested() => OnModalHideRequested?.Invoke();
}