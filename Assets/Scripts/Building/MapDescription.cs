using System;
using Scripts.Building.Tile;
using Scripts.ScenesManagement;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class MapDescription
    {
        public Vector3Int StartPosition;
        public Quaternion PlayerRotation;
        public string SceneName;
        public TileDescription[,] Layout;
        public string MapName = "DefaultMapName";

        public MapDescription()
        {
            StartPosition = DefaultMapProvider.StartPosition;
            PlayerRotation = Quaternion.identity;
            SceneName = Scenes.PlayIndoorSceneName;
        }
    }
}
