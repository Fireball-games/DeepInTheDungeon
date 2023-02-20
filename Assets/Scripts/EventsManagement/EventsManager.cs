using System;
using Scripts.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.EventsManagement
{
    public class EventsManager : MonoBehaviour
    {
        public static UnityEvent OnNewCampaignStarted = new();
        public static event Action OnLevelStarted;
        public static event Action OnSceneStartedLoading;
        public static event Action<string> OnSceneFinishedLoading;
        public static event Action<Vector3> OnPlayerRotationChanged; 
        public static event Action<Vector3> OnPlayerPositionChanged;
        public static event Action<Trigger> OnTriggerNext;
        public static event Action OnMapDemolished;
        public static event Action<string> OnMapTraversalTriggered;

        // ***********    Triggers    ***********

        public static void TriggerOnNewCampaignStarted() => OnNewCampaignStarted.Invoke();
        public static void TriggerOnLevelStarted() => OnLevelStarted?.Invoke();
        public static void TriggerOnSceneStartedLoading() => OnSceneStartedLoading?.Invoke();
        public static void TriggerOnSceneFinishedLoading(string sceneName) => OnSceneFinishedLoading?.Invoke(sceneName);
        public static void TriggerOnPlayerRotationChanged(Vector3 targetRotation) => OnPlayerRotationChanged?.Invoke(targetRotation);
        public static void TriggerOnPlayerPositionChanged(Vector3 newPosition) => OnPlayerPositionChanged?.Invoke(newPosition);
        public static void TriggerOnTriggerNext(Trigger source ) => OnTriggerNext?.Invoke(source);
        public static void TriggerOnMapDemolished() => OnMapDemolished?.Invoke();
        public static void TriggerOnMapTraversalTriggered(string guid) => OnMapTraversalTriggered?.Invoke(guid);
    }
}