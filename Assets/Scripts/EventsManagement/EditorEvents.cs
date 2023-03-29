using System;
using Scripts.Building.ItemSpawning;
using Scripts.InventoryManagement.Inventories;
using UnityEngine;
using UnityEngine.Events;
using static Scripts.MapEditor.Enums;

namespace Scripts.EventsManagement
{
    public static class EditorEvents
    {
        public static event Action OnNewMapStartedCreation;
        public static readonly UnityEvent OnMapBuilt = new();
        public static event Action<EWorkMode> OnWorkModeChanged;
        public static event Action<Vector3Int, Vector3Int> OnMouseGridPositionChanged;
        public static event Action<ELevel> OnWorkingLevelChanged;
        public static readonly UnityEvent<EEditMode> OnEditModeChanged = new();
        public static event Action<int?> OnFloorChanged;
        public static event Action OnMapLayoutChanged;
        public static event Action<bool> OnPrefabEdited;
        public static event Action OnMapSaved;
        public static event Action<bool> OnCameraPerspectiveChanged;
        public static readonly UnityEvent<Vector3> OnAddItemToMap = new();
        public static readonly UnityEvent<MapObjectInstance> OnMouseEnterMapObject = new();
        public static readonly UnityEvent<MapObjectInstance> OnMouseExitMapObject = new();

        // ***********    Triggers    ***********

        public static void TriggerOnNewMapStartedCreation() => OnNewMapStartedCreation?.Invoke();
        public static void TriggerOnMapBuilt() => OnMapBuilt.Invoke();
        public static void TriggerOnWorkModeChanged(EWorkMode workMode) => OnWorkModeChanged?.Invoke(workMode);
        public static void TriggerOnEditModeChanged(EEditMode editMode) => OnEditModeChanged.Invoke(editMode);
        public static void TriggerOnMouseGridPositionChanged(Vector3Int newGridPosition, Vector3Int previousGridPosition) =>
            OnMouseGridPositionChanged?.Invoke(newGridPosition, previousGridPosition);
        public static void TriggerOnWorkingLevelChanged(ELevel workingLevel) => OnWorkingLevelChanged?.Invoke(workingLevel);
        public static void TriggerOnFloorChanged(int? floor) => OnFloorChanged?.Invoke(floor);
        public static void TriggerOnMapLayoutChanged() => OnMapLayoutChanged?.Invoke();
        public static void TriggerOnPrefabEdited(bool isEdited) => OnPrefabEdited?.Invoke(isEdited);
        public static void TriggerOnMapSaved() => OnMapSaved?.Invoke();
        public static void TriggerOnCameraPerspectiveChanged(bool isOrthographic) => OnCameraPerspectiveChanged?.Invoke(isOrthographic);
        public static void TriggerOnAddItemToMap(Vector3 position) => OnAddItemToMap.Invoke(position);
        public static void TriggerOnMouseEnterMapObject(MapObjectInstance mapObject) => OnMouseEnterMapObject.Invoke(mapObject);
        public static void TriggerOnMouseExitMapObject(MapObjectInstance mapObject) => OnMouseExitMapObject.Invoke(mapObject);
    }
}