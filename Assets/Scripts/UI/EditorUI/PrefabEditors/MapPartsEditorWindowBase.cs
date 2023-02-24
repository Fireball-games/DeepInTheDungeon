using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using static Scripts.MapEditor.Enums;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public abstract class MapPartsEditorWindowBase : EditorWindowBase, IPrefabEditor
    {
        [Tooltip("Specifies for what work more is this editor intended")]
        [SerializeField] private EWorkMode workMode;
        
        protected MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        protected CageController SelectedCursor => EditorUIManager.Instance.SelectedCage;
        
        private void OnEnable()
        {
            EditorEvents.OnWorkModeChanged += Close;
        }

        private void OnDisable()
        {
            EditorEvents.OnWorkModeChanged -= Close;
            StopAllCoroutines();
        }
        
        public abstract void Open();

        public virtual void CloseWithRemovingChanges() => RemoveAndClose();

        protected abstract void RemoveAndClose();

        public virtual void MoveCameraToPrefab(Vector3 targetPosition) =>
            EditorCameraService.Instance.MoveCameraToPrefab(Vector3Int.RoundToInt(targetPosition));

        public virtual Vector3 GetCursor3DScale() => Vector3.one;
        
        private void Close(EWorkMode newWorkMode)
        {
            if (newWorkMode != workMode) RemoveAndClose();
        }
    }
}