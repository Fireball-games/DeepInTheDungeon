using System.Collections.Generic;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI
{
    public class FloorManagement : UIElementBase
    {
        [SerializeField] private GameObject floorButtonPrefab;
        [SerializeField] private ImageButton upButton;
        [SerializeField] private ImageButton downButton;

        private List<FloorButton> _floorButtons;

        private MapEditorManager Manager => MapEditorManager.Instance;

        private int _floorCount;
        private int _currentFloor;

        private void Awake()
        {
            _floorButtons = new List<FloorButton>();
        }

        private void OnEnable()
        {
            EditorEvents.OnNewMapStartedCreation += ConstructButtons;
            EditorEvents.OnFloorChanged += OnFloorChanged;

            upButton.OnClick += ChangeGridFloorUp;
            downButton.OnClick += ChangeGridFloorDown;
        }

        private void OnDisable()
        {
            EditorEvents.OnNewMapStartedCreation -= ConstructButtons;
            EditorEvents.OnFloorChanged -= OnFloorChanged;

            upButton.OnClick -= ChangeGridFloorUp;
            downButton.OnClick -= ChangeGridFloorDown;
        }

        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);

            ConstructButtons();
        }

        private void ConstructButtons()
        {
            upButton.transform.SetParent(transform);
            downButton.transform.SetParent(transform);

            body.DismissAllChildrenToPool(true);

            _floorButtons.Clear();

            MapDescription map = Manager.MapBuilder.MapDescription;
            _floorCount = map.Layout.GetLength(0);
            _currentFloor = Manager.CurrentFloor;

            upButton.SetActive(true);
            upButton.transform.SetParent(body.transform);

            AddFloorButtons();

            downButton.SetActive(true);
            downButton.transform.SetParent(body.transform);

            SetInteractivity();
        }

        private void OnFloorChanged(int? currentFloor)
        {
            _currentFloor = currentFloor ?? 0;
            SetInteractivity();
        }

        private void SetInteractivity()
        {
            if (_floorCount <= 3)
            {
                _floorButtons[0].SetSelected(true, true);
                _floorButtons[0].SetInteractable(false);
            }
            else
            {
                foreach (FloorButton floorButton in _floorButtons)
                {
                    floorButton.SetSelected(floorButton.Floor == _currentFloor, true);
                    floorButton.SetVisibilityToggle();
                }
            }

            upButton.SetInteractable(_currentFloor != 1);

            downButton.SetInteractable(_currentFloor != _floorCount - 2);
        }

        private void AddFloorButtons()
        {
            for (int i = 0; i < _floorCount - 2; i++)
            {
                FloorButton newButton = ObjectPool.Instance.GetFromPool(floorButtonPrefab, body, true)
                    .GetComponent<FloorButton>();

                newButton.SetActive(true, (i + 1));

                newButton.SetSelected(_currentFloor == i + 1, true);

                _floorButtons.Add(newButton);
            }
        }

        /// <summary>
        /// Changes current floor grid-wise, so top floor is 0
        /// </summary>
        private void ChangeGridFloorUp() => Manager.SetFloor(Manager.CurrentFloor - 1);

        /// <summary>
        /// Changes current floor grid-wise, so top floor is 0
        /// </summary>
        private void ChangeGridFloorDown() => Manager.SetFloor(Manager.CurrentFloor + 1);
    }
}