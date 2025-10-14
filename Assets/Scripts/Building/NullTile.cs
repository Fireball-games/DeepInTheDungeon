using System;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Building
{
    [SelectionBase]
    public class NullTile : MonoBehaviour, IPoolInitializable
    {
        [SerializeField] protected MeshRenderer bodyRenderer;
        [SerializeField] private GameObject FloorObject;
        public Material normalMaterial;
        public Material transparentMaterial;
        public bool isOnEdgeAboveGround;

        private static GameManager Manager => GameManager.Instance;
        private bool IsOnUpperFloor => Math.Abs(-transform.position.y - (MapEditorManager.Instance.CurrentFloor - 1)) < float.Epsilon;
        private bool IsOutdoorMap => Manager.MapBuilder.MapDescription.IsOutdoor;
        private bool IsAboveGround => Manager.MapBuilder.IsOnOrAboveGroundLevel(transform.position.ToGridPosition().x);
        private bool IsEdgeTile => Manager.MapBuilder.IsEdgeTile(transform.position.ToGridPosition());
        private bool IsOnGroundLevel => Manager.MapBuilder.IsOnGroundLevel(transform.position.ToGridPosition().x);

        private int _myFloor;

        public void InitializeFromPool()
        {
            ShowTile();
        }
        
        public void Initialize()
        {
            isOnEdgeAboveGround = IsOutdoorMap && IsAboveGround && IsEdgeTile;
            ShowTile();
        }

        public void ShowTile(bool show = true)
        {
            if (isOnEdgeAboveGround)
            {
                SetBodyMaterial(transparentMaterial);
                FloorObject.gameObject.SetActive(IsOnGroundLevel && show);
            }
            else
            {
                SetBodyMaterial(normalMaterial);
                FloorObject.gameObject.SetActive(false);
            }
            
            bodyRenderer.enabled = show;
        }
        
        public void ShowFloorOnly(bool show = true)
        {
            bodyRenderer.enabled = false;
            FloorObject.gameObject.SetActive(show);
        }

        private void OnTriggerEnter(Collider other)
        {
            _myFloor = Mathf.RoundToInt(transform.position.y);

            if (!IsOnUpperFloor || other.gameObject.layer != LayersManager.UpperFloor) return;

            bodyRenderer.enabled = true;
            SetBodyMaterial(transparentMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsOnUpperFloor) return;

            ShowTile(!Manager.MapBuilder.ShouldBeInvisible(-_myFloor));
        }

        protected void SetBodyMaterial(Material material) => bodyRenderer.material = material;
    }
}