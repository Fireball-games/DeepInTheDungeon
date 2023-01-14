using System;
using Scripts.MapEditor;
using Scripts.System.Pooling;
using UnityEngine;

public class NullTile : MonoBehaviour, IPoolInitializable
{
    [SerializeField] private MeshRenderer bodyRenderer;
    public Material normalMaterial;
    public Material transparentMaterial;

    private MapEditorManager Manager => MapEditorManager.Instance;
    private bool _isOnUpperFloor => Math.Abs(-transform.position.y - (Manager.CurrentFloor - 1)) < float.Epsilon;

    private int _myFloor;

    public void InitializeFromPool()
    {
        ShowTile();
    }

    public void ShowTile(bool show = true)
    {
        bodyRenderer.enabled = show;
        SetMaterial(normalMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        _myFloor = Mathf.RoundToInt(transform.position.y);
        
        if (_isOnUpperFloor)
        {
            bodyRenderer.enabled = true;
            SetMaterial(transparentMaterial);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_isOnUpperFloor) return;

        ShowTile(!Manager.MapBuilder.ShouldBeInvisible(-_myFloor));
    }

    private void SetMaterial(Material material) => bodyRenderer.material = material;
}