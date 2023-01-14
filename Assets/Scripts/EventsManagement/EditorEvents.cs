using System;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.EventsManagement
{
    public static class EditorEvents
    {
        public static event Action OnNewMapStartedCreation;
        public static event Action<EWorkMode> OnWorkModeChanged;
        public static event Action<Vector3Int, Vector3Int> OnMouseGridPositionChanged;
        public static event Action<ELevel> OnWorkingLevelChanged;
        public static event Action<ETriggerEditMode> OnTriggerWorkModeChanged;
        public static event Action<int?> OnFloorChanged;
        public static event Action<bool> OnMapEditedStatusChanged;
        public static event Action<bool> OnCameraPerspectiveChanged;

        // ***********    Triggers    ***********

        public static void TriggerOnNewMapStartedCreation() => OnNewMapStartedCreation?.Invoke();

        public static void TriggerOnWorkModeChanged(EWorkMode workMode) => OnWorkModeChanged?.Invoke(workMode);

        public static void TriggerOnMouseGridPositionChanged(Vector3Int newGridPosition, Vector3Int previousGridPosition) =>
            OnMouseGridPositionChanged?.Invoke(newGridPosition, previousGridPosition);

        public static void TriggerOnWorkingLevelChanged(ELevel workingLevel) => OnWorkingLevelChanged?.Invoke(workingLevel);

        public static void TriggerOnFloorChanged(int? floor) => OnFloorChanged?.Invoke(floor);

        public static void TriggerOnMapEditedStatusChanged(bool isEdited) => OnMapEditedStatusChanged?.Invoke(isEdited);

        public static void TriggerOnCameraPerspectiveChanged(bool isOrthographic) => OnCameraPerspectiveChanged?.Invoke(isOrthographic);
        public static void TriggerOnTriggerWorkModeChanged(ETriggerEditMode triggerWorkMode) => OnTriggerWorkModeChanged?.Invoke(triggerWorkMode);
    }
}