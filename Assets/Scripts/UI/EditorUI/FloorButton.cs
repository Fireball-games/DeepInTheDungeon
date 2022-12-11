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
        [NonSerialized] public int Floor; 

        public void SetActive(bool isActive, int floorNumber)
        {
            Floor = floorNumber;
            textField.text = floorNumber.ToString();
            OnClick += ChangeFloor;

            base.SetActive(isActive);
        }

        private void OnDisable()
        {
            OnClick -= ChangeFloor;
        }

        private void ChangeFloor()
        {
            MapEditorManager.Instance.SetFloor(Floor);
        }
    }
}