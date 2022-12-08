using System;
using Scripts.Building.Tile;
using Scripts.ScenesManagement;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class MapDescription
    {
        /// <summary>
        /// Position according to Layout array
        /// </summary>
        public Vector3Int StartGridPosition;
        public Quaternion PlayerRotation;
        public string SceneName;
        public TileDescription[,,] Layout;
        public string MapName = "DefaultMapName";

        public MapDescription()
        {
            
            StartGridPosition = DefaultMapProvider.StartPosition;
            PlayerRotation = Quaternion.identity;
            SceneName = Scenes.PlayIndoorSceneName;
        }
    }
}
