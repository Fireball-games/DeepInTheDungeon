using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EditorStartPointEditor : EditorWindowBase, IPrefabEditor
    {
        private CageController SelectedCursor => EditorUIManager.Instance.SelectedCage;
        private RotationWidget _rotationWidget;
        private ImageButton _searchButton;
        
        private static MapEditorManager Manager => MapEditorManager.Instance;
        private static EditorCameraService CameraService => EditorCameraService.Instance;
        
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _indicatorTransform;
        
        private EditorStartIndicator _editorStartIndicator;
        
        private Title _title;
        private Button _saveButton;
        private Button _cancelButton;
        
        private bool _startPositionChanged;
        private int _originalFloorForNavigation;
        private PositionRotation _navigationPositionRotation;

        private void Awake()
        {
            AssignComponents();
        }

        public void Open()
        {
            SetActive(true);
            SetButtons();
            _originalPosition = _indicatorTransform.position;
            _originalRotation = _indicatorTransform.rotation;
        }
        
        public void Open(PositionRotation placeholderTransformData)
        {
            SetActive(true);
            SetButtons();
            SetEdited(true);
            SelectedCursor.ShowAt(placeholderTransformData.Position, Vector3.one, Quaternion.identity);
            _originalPosition = _indicatorTransform.position;
            _originalRotation = _indicatorTransform.rotation;
            _editorStartIndicator.SetPositionByWorld(placeholderTransformData.Position);
        }

        public void CloseWithRemovingChanges()
        {
            _editorStartIndicator.SetPositionByWorld(_originalPosition);
            _editorStartIndicator.SetArrowRotation(_originalRotation);
            SetActive(false);
        }

        public void MoveCameraToPrefab(Vector3 worldPosition) => CameraService.MoveCameraToPrefab(worldPosition);

        public Vector3 GetCursor3DScale() => Vector3.one;
        
        private void OnRotated(int direction)
        {
            SetEdited(true);
            _editorStartIndicator.SetArrowRotationYDelta(direction * 90f);
        }

        private void SetEdited(bool isEdited)
        {
            _startPositionChanged = isEdited;
            
            EditorUIManager.Instance.SetAnyObjectEdited(isEdited);
            EditorEvents.TriggerOnPrefabEdited(isEdited);
            
            SetButtons();
        }
        
        private void SetButtons()
        {
            _saveButton.gameObject.SetActive(_startPositionChanged);
            _saveButton.SetText(t.Get(Keys.Save));
            _cancelButton.gameObject.SetActive(_startPositionChanged);
            _cancelButton.SetText(t.Get(Keys.Cancel));
        }
        
        private void RemoveChanges()
        {
            _editorStartIndicator.SetPositionByWorld(_originalPosition);
            _editorStartIndicator.SetArrowRotation(_originalRotation);
            SelectedCursor.Hide();
            SetEdited(false);
            SetButtons();
        }
        
        private void Save()
        {
            MapDescription currentMap = GameManager.Instance.CurrentMap;
            _originalPosition = _indicatorTransform.position;
            _originalRotation = _indicatorTransform.rotation;
            _editorStartIndicator.SetPositionInMapAndWorld(_editorStartIndicator.transform.position);
            currentMap.EditorPlayerStartRotation = _editorStartIndicator.GetPlayerMapRotation();
            Manager.SaveMap();
            SetEdited(false);
            SelectedCursor.Hide();
        }
        
        private void OnSearchButtonMouseExit()
        {
            Manager.SetFloor(_originalFloorForNavigation);
            CameraService.MoveCameraTo(_navigationPositionRotation);
        }

        private void OnSearchButtonMouseEnter()
        {
            _originalFloorForNavigation = Manager.CurrentFloor;
            _navigationPositionRotation = CameraService.GetCameraTransformData();
            CameraService.MoveCameraToPrefab(_indicatorTransform.position);
        }

        private void OnSearchButtonClicked()
        {
            _originalFloorForNavigation = Manager.CurrentFloor;
            _navigationPositionRotation = CameraService.GetCameraTransformData();
            CameraService.MoveCameraTo(_navigationPositionRotation);
        }

        private void AssignComponents()
        {
            _editorStartIndicator = FindObjectOfType<EditorStartIndicator>();
            _indicatorTransform = _editorStartIndicator.transform;
            
            _rotationWidget = body.transform.Find("Background/Frame/Content/RotationWidget").GetComponent<RotationWidget>();
            _rotationWidget.SetUp(t.Get(Keys.Rotate), () => OnRotated(-1), () => OnRotated(1));
            
            Transform frame = body.transform.Find("Background/Frame");
            _title = frame.Find("Header/PrefabTitle").GetComponent<Title>();
            _title.SetTitle(t.Get(Keys.EditorStartPoint));
            
            _saveButton = frame.Find("Buttons/SaveButton").GetComponent<Button>();
            _saveButton.SetTextColor(Colors.Positive);
            _saveButton.onClick.AddListener(Save);
            
            _cancelButton = frame.Find("Buttons/CancelButton").GetComponent<Button>();
            _cancelButton.SetTextColor(Colors.Warning);
            _cancelButton.onClick.AddListener(RemoveChanges);
            
            _searchButton = frame.Find("Header/SearchButton").GetComponent<ImageButton>();
            _searchButton.OnClick.AddListener(OnSearchButtonClicked);
            _searchButton.OnMouseEnter.AddListener(OnSearchButtonMouseEnter);
            _searchButton.OnMouseExit.AddListener(OnSearchButtonMouseExit);
            
        }
    }
}