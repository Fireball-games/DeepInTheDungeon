using System;
using Scripts.MapEditor;
using Scripts.MapEditor.Services;
using Scripts.UI.Components.Buttons;
using UnityEngine;

namespace Scripts.UI.EditorUI.Components
{
    public class FloorButton : TextButton
    {
        [SerializeField] private ToggleFramedButton visibilityButton; 
        [NonSerialized] public int Floor; 

        public void SetActive(bool isActive, int floorNumber)
        {
            Floor = floorNumber;
            textField.text = floorNumber.ToString();
            OnClick.RemoveAllListeners();
            OnClick.AddListener(ChangeFloor);
            visibilityButton.OnToggleOn += OnVisibilityButtonToggleOn;
            visibilityButton.OnToggleOff += OnVisibilityButtonToggleOff;

            SetVisibilityToggle();

            base.SetActive(isActive);
        }
        
        public void SetVisibilityToggle()
        {
            if (MapEditorManager.Instance.FloorVisibilityMap[Floor])
            {
                visibilityButton.ToggleOn(true);
            }
            else
            {
                visibilityButton.ToggleOff(true);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            visibilityButton.OnToggleOn -= OnVisibilityButtonToggleOn;
            visibilityButton.OnToggleOff -= OnVisibilityButtonToggleOff;
        }

        private void ChangeFloor()
        {
            MapEditorManager.Instance.SetFloor(Floor);
        }

        private void OnVisibilityButtonToggleOn()
        {
            MapBuildService.SetFloorVisible(Floor, true);
            MapEditorManager.Instance.MapBuilder.SetPrefabsVisibility();
        }
        
        private void OnVisibilityButtonToggleOff()
        {
            MapBuildService.SetFloorVisible(Floor, false);
            MapEditorManager.Instance.MapBuilder.SetPrefabsVisibility();
        }
    }
}