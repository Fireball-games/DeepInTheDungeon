﻿using System;
using System.Collections.Generic;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Building.Tile.TileDescription;
using Logger = Scripts.Helpers.Logger;


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

        public void HideWall(ETileDirection direction) => wallMap[direction].wallParent.SetActive(false);

        public void ShowWall(ETileDirection direction) => wallMap[direction].wallParent.SetActive(true);

        public void SetMeshAndMaterial(ETileDirection direction, Material material, Mesh mesh)
        {
            SetMaterial(direction, material);
            SetMesh(direction, mesh);
        }

        public void SetMaterial(ETileDirection direction, Material material) => wallMap[direction].renderer.material = material;

        public void SetMesh(ETileDirection direction, Mesh mesh) => wallMap[direction].meshFilter.mesh = mesh;
        
        public void Initialize()
        {
            Logger.Log("Tile is being reset.");
            transform.localScale = Vector3.one;

            foreach (TileRenderingParts wall in wallMap.Values)
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