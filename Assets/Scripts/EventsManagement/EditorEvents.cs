using System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.EventsManagement
{
    public static class EditorEvents
    {
        public static event Action OnNewMapCreated;
        public static event Action<EWorkMode> OnWorkModeChanged;

        public static event Action<Vector3Int, Vector3Int> OnMouseGridPositionChanged; 

        // ***********    Triggers    ***********

        public static void TriggerOnNewMapCreated() => OnNewMapCreated?.Invoke();

        public static void TriggerOnWorkModeChanged(EWorkMode workMode) => OnWorkModeChanged?.Invoke(workMode);

        public static void TriggerOnMouseGridPositionChanged(Vector3Int newPosition, Vector3Int previousPosition) =>
            OnMouseGridPositionChanged?.Invoke(newPosition, previousPosition);
    }
}