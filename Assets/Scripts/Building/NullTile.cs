using System;
using Scripts.MapEditor;
using Scripts.System.Pooling;
using UnityEngine;

public class NullTile : MonoBehaviour, IPoolInitializable
{
    [SerializeField] private MeshRenderer bodyRenderer;
    public Color fullColor = new(1, 1, 1, 1);
    public Color nearTransparentColor = new(0.5f, 1, 0.5f, 0.65f);

    private MapEditorManager Manager => MapEditorManager.Instance;
    private bool _isOnUpperFloor => Math.Abs(-transform.position.y - (Manager.CurrentFloor - 1)) < float.Epsilon;

    private int _myFloor;

    public void Initialize()
    {
        ShowTile();
    }

    public void ShowTile(bool show = true)
    {
        bodyRenderer.enabled = show;
        SetColor(fullColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        _myFloor = Mathf.RoundToInt(transform.position.y);
        
        if (_isOnUpperFloor)
        {
            bodyRenderer.enabled = true;
            SetColor(nearTransparentColor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_isOnUpperFloor) return;

        ShowTile(!Manager.MapBuilder.ShouldBeInvisible(-_myFloor));
    }

    private void SetColor(Color newColor) => bodyRenderer.material.color = newColor;
}