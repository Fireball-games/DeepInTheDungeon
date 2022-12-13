using System;
using System.Collections.Generic;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;


namespace Scripts.Building.Tile
{
    [SelectionBase]
    public class TileController : MonoBehaviour, IPoolInitializable
    {
        [SerializeField] private TileRenderingParts floor;
        [SerializeField] private TileRenderingParts ceiling;
        [SerializeField] private TileRenderingParts northWall;
        [SerializeField] private TileRenderingParts eastWall;
        [SerializeField] private TileRenderingParts southWall;
        [SerializeField] private TileRenderingParts westWall;
        
        private Dictionary<ETileDirection, TileRenderingParts> _wallMap;
        
        private void Awake()
        {
            _wallMap = new Dictionary<ETileDirection, TileRenderingParts>
            {
                {ETileDirection.Floor, floor},
                {ETileDirection.Ceiling, ceiling},
                {ETileDirection.North, northWall},
                {ETileDirection.East, eastWall},
                {ETileDirection.South, southWall},
                {ETileDirection.West, westWall},
            };
        }

        public void HideWall(ETileDirection direction) => _wallMap[direction].wallParent.SetActive(false);

        public void ShowWall(ETileDirection direction) => _wallMap[direction].wallParent.SetActive(true);

        public void SetMeshAndMaterial(ETileDirection direction, Material material, Mesh mesh)
        {
            SetMaterial(direction, material);
            SetMesh(direction, mesh);
        }

        public void SetMaterial(ETileDirection direction, Material material) => _wallMap[direction].renderer.material = material;

        public void SetMesh(ETileDirection direction, Mesh mesh) => _wallMap[direction].meshFilter.mesh = mesh;
        
        public void Initialize()
        {
            transform.localScale = Vector3.one;
            // TODO: probably OK to remove once map layers are implemented
            foreach (TileRenderingParts wall in _wallMap.Values)
            {
                wall.wallParent.SetActive(true);
            }
        }
    }

    [Serializable]
    internal class TileRenderingParts
    {
        public GameObject wallParent;
        public MeshFilter meshFilter;
        public MeshRenderer renderer;
    }
}