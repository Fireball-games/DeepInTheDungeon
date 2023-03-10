using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Scripts.UI.EditorUI.PrefabEditors.WaypointEditor;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class AddWaypointWidget : MonoBehaviour
    {
        private TMP_Text _label;
        private Button _upButton;
        private Button _downButton;
        private Button _northButton;
        private Button _eastButton;
        private Button _southButton;
        private Button _westButton;

        private Dictionary<Button, Vector3> _map;
        private EAddWaypointType _type;

        private UnityEvent<Vector3, EAddWaypointType> OnAddButtonClicked { get; set; } = new();

        public void Set(string labelText, EAddWaypointType type, UnityAction<Vector3, EAddWaypointType> onAddButtonClick)
        {
            Initialize();
            _label.text = string.IsNullOrEmpty(labelText) ? "" : labelText;
            _type = type;
            OnAddButtonClicked.RemoveAllListeners();
            OnAddButtonClicked.AddListener(onAddButtonClick);
        }
        
        private void Initialize()
        {
            _map = new Dictionary<Button, Vector3>();
            
            _label = transform.Find("Label").GetComponent<TMP_Text>();
            
            _upButton = transform.Find("Buttons/UpButton").GetComponent<Button>();
            SetButton(_upButton, Vector3.up, "");
            
            _downButton = transform.Find("Buttons/DownButton").GetComponent<Button>();
            SetButton(_downButton, Vector3.down, "");
            
            _northButton = transform.Find("Buttons/NorthButton").GetComponent<Button>();
            SetButton(_northButton, GeneralExtensions.WorldNorth, "");
            
            _eastButton = transform.Find("Buttons/EastButton").GetComponent<Button>();
            SetButton(_eastButton, GeneralExtensions.WorldEast, "");
            
            _southButton = transform.Find("Buttons/SouthButton").GetComponent<Button>();
            SetButton(_southButton, GeneralExtensions.WorldSouth, "");
            
            _westButton = transform.Find("Buttons/WestButton").GetComponent<Button>();
            SetButton(_westButton, GeneralExtensions.WorldWest, "");
        }

        private void SetButton(Button button, Vector3 direction, string labelText)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnAddButtonClicked_internal(button));
            _map.Add(button, direction);
            button.GetComponentInChildren<TMP_Text>().text = labelText;
        }

        private void OnAddButtonClicked_internal(Button clickedButton)
        {
            OnAddButtonClicked.Invoke(_map[clickedButton], _type);
        }
    }
}
