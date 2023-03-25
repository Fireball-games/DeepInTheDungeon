using System;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Building
{
    public class NullTile : MonoBehaviour, IPoolInitializable
    {
        [SerializeField] private MeshRenderer bodyRenderer;
        public Material normalMaterial;
        public Material transparentMaterial;

        private MapEditorManager Manager => MapEditorManager.Instance;
        private bool IsOnUpperFloor => Math.Abs(-transform.position.y - (Manager.CurrentFloor - 1)) < float.Epsilon;

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

            if (!IsOnUpperFloor || other.gameObject.layer != LayersManager.UpperFloor) return;

            bodyRenderer.enabled = true;
            SetMaterial(transparentMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsOnUpperFloor) return;

            ShowTile(!Manager.MapBuilder.ShouldBeInvisible(-_myFloor));
        }

        private void SetMaterial(Material material) => bodyRenderer.material = material;
    }
}