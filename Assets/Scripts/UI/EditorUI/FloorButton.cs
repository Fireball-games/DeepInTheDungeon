using System;
using Scripts.MapEditor;
using Scripts.UI.Components;
using TMPro;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class FloorButton : ImageButton
    {
        [SerializeField] private TMP_Text textField;
        [SerializeField] private ToggleFramedButton visibilityButton; 
        [NonSerialized] public int Floor; 

        public void SetActive(bool isActive, int floorNumber)
        {
            Floor = floorNumber;
            textField.text = floorNumber.ToString();
            OnClick += ChangeFloor;
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
            
            OnClick -= ChangeFloor;
            visibilityButton.OnToggleOn += OnVisibilityButtonToggleOn;
            visibilityButton.OnToggleOff += OnVisibilityButtonToggleOff;
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