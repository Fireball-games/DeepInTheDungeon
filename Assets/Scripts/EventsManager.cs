using System;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public static event Action OnStartGameRequested;
    public static event Action OnOpenEditorRequested;
    public static event Action OnLevelStarted;

    public static void TriggerOnStartGameRequested() => OnStartGameRequested?.Invoke();
    public static void TriggerOnOpenEditorRequested() => OnOpenEditorRequested?.Invoke();
    public static void TriggerOnLevelStarted() => OnLevelStarted?.Invoke();
}