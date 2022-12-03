using System;
using System.Collections.Generic;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.Building.Tile
{
    public class TileController : MonoBehaviour
    {
        [SerializeField] private TileRenderingParts floor;
        [SerializeField] private TileRenderingParts ceiling;
        [SerializeField] private TileRenderingParts northWall;
        [SerializeField] private TileRenderingParts eastWall;
        [SerializeField] private TileRenderingParts southWall;
        [SerializeField] private TileRenderingParts westWall;
        
        private Dictionary<ETileDirection, TileRenderingParts> wallMap;
        
        private void Awake()
        {
            wallMap = new Dictionary<ETileDirection, TileRenderingParts>
            {
                {ETileDirection.Floor, floor},
                {ETileDirection.Ceiling, ceiling},
                {ETileDirection.North, northWall},
                {ETileDirection.East, eastWall},
                {ETileDirection.South, southWall},
                {ETileDirection.West, westWall},
            };
        }

        public void RemoveWall(ETileDirection direction)
        {
            Destroy(wallMap[direction].renderer.gameObject);
        }

        public void SetMeshAndMaterial(ETileDirection direction, Material material, Mesh mesh)
        {
            SetMaterial(direction, material);
            SetMesh(direction, mesh);
        }

        public void SetMaterial(ETileDirection direction, Material material) => wallMap[direction].renderer.material = material;

        public void SetMesh(ETileDirection direction, Mesh mesh) => wallMap[direction].meshFilter.mesh = mesh;
    }

    [Serializable]
    internal class TileRenderingParts
    {
        public MeshFilter meshFilter;
        public MeshRenderer renderer;
    }
}