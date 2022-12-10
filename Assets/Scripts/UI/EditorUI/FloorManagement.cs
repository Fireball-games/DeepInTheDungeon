using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using Scripts.UI.EditorUI;
using UnityEngine;

public class FloorManagement : UIElementBase
{
    [SerializeField] private GameObject upButtonPrefab;
    [SerializeField] private GameObject downButtonPrefab;
    [SerializeField] private GameObject floorButtonPrefab;

    private ImageButton _upButton;
    private ImageButton _downButton;
    private List<FloorButton> _floorButtons;

    private int _floorCount;
    private int _currentFloor;

    private enum EButton
    {
        Up,
        Down,
        Floor
    };

    private void Awake()
    {
        _floorButtons = new List<FloorButton>();
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);
        
        ConstructButtons();
    }

    private void ConstructButtons()
    {
        body.DismissAllChildrenToPool(true);
        _floorButtons.Clear();
        
        MapDescription map = MapEditorManager.Instance.MapBuilder.MapDescription;
        _floorCount = map.Layout.GetLength(0);
        _currentFloor = map.StartGridPosition.x;

        GetButton(EButton.Up);
        AddFloorButtons();
        GetButton(EButton.Down);
    }

    private GameObject GetButton(EButton whichButton) => whichButton switch
    {
        EButton.Up => ObjectPool.Instance.GetFromPool(upButtonPrefab, body, true),
        EButton.Down => ObjectPool.Instance.GetFromPool(downButtonPrefab, body, true),
        EButton.Floor => ObjectPool.Instance.GetFromPool(floorButtonPrefab, body, true),
        _ => throw new ArgumentOutOfRangeException(nameof(whichButton), whichButton, null)
    };

    private void AddFloorButtons()
    {
        for (int i = 0; i < _floorCount - 2; i++)
        {
            FloorButton newButton = ObjectPool.Instance.GetFromPool(floorButtonPrefab, body, true)
                .GetComponent<FloorButton>();
            
            newButton.SetActive(true, (i + 1).ToString());
            
            _floorButtons.Add(newButton);
        }
    }

}
