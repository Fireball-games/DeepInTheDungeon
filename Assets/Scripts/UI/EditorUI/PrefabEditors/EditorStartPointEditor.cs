using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EditorStartPointEditor : EditorWindowBase, IPrefabEditor
    {
        private RotationWidget _rotationWidget;
        
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _indicatorTransform;
        
        private EditorStartIndicator _editorStartIndicator;

        private void Awake()
        {
            _editorStartIndicator = FindObjectOfType<EditorStartIndicator>();
            _indicatorTransform = _editorStartIndicator.transform;
            
            _rotationWidget = body.transform.Find("Background/Frame/Content/RotationWidget").GetComponent<RotationWidget>();
            _rotationWidget.SetUp(t.Get(Keys.Rotate), () => OnRotated(1), () => OnRotated(1));
        }

        public void Open()
        {
            SetActive(true);
            _originalPosition = _indicatorTransform.position;
            _originalRotation = _indicatorTransform.rotation;
        }

        public void CloseWithRemovingChanges()
        {
            _editorStartIndicator.SetPositionByWorld(_originalPosition);
            _editorStartIndicator.SetArrowRotation(_originalRotation);
        }

        public void MoveCameraToPrefab(Vector3 worldPosition) => EditorCameraService.Instance.MoveCameraToPrefab(worldPosition);

        public Vector3 GetCursor3DScale() => Vector3.one;
        
        private void OnRotated(int direction) => _editorStartIndicator.SetArrowRotationYDelta(direction * 90f);
    }
}