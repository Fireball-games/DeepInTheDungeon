using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.EventsManagement
{
    public class EventsManager : MonoBehaviour
    {
        public static event Action OnStartGameRequested;
        public static event Action OnLevelStarted;
        public static event Action OnSceneStartedLoading;
        public static event Action<string> OnSceneFinishedLoading;
        public static event Action OnModalShowRequested;
        public static event Action OnModalHideRequested;
        public static event Action OnModalClicked;
        public static event Action<Vector3> OnPlayerRotationChanged; 
        public static event Action<Vector3> OnPlayerPositionChanged;
        public static event Action<List<string>> OnTriggerNext; 

        // ***********    Triggers    ***********

        public static void TriggerOnStartGameRequested() => OnStartGameRequested?.Invoke();
        public static void TriggerOnLevelStarted() => OnLevelStarted?.Invoke();
        public static void TriggerOnSceneStartedLoading() => OnSceneStartedLoading?.Invoke();
        public static void TriggerOnSceneFinishedLoading(string sceneName) => OnSceneFinishedLoading?.Invoke(sceneName);
        public static void TriggerOnModalShowRequested() => OnModalShowRequested?.Invoke();
        public static void TriggerOnModalHideRequested() => OnModalHideRequested?.Invoke();
        public static void TriggerOnModalClicked() => OnModalClicked?.Invoke();
        public static void TriggerOnPlayerRotationChanged(Vector3 targetRotation) => OnPlayerRotationChanged?.Invoke(targetRotation);
        public static void TriggerOnPlayerPositionChanged(Vector3 newPosition) => OnPlayerPositionChanged?.Invoke(newPosition);
        public static void TriggerOnTriggerNext(List<string> subscribers) => OnTriggerNext?.Invoke(subscribers);
    }
}