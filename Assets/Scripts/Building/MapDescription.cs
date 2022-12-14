using Scripts.Building.Tile;
using Scripts.Building.Walls.Configurations;
using Scripts.ScenesManagement;
using UnityEngine;

namespace Scripts.Building
{
    public class MapDescription
    {
        public string MapName = "DefaultMapName";
        /// <summary>
        /// Position according to Layout array
        /// </summary>
        public Vector3Int StartGridPosition;
        public Quaternion PlayerRotation;
        public string SceneName;
        public TileDescription[,,] Layout;
        public PrefabConfiguration[] PrefabConfigurations;

        public MapDescription()
        {
            
            StartGridPosition = Vector3Int.zero;
            PlayerRotation = Quaternion.identity;
            SceneName = Scenes.PlayIndoorSceneName;
        }
    }
}
