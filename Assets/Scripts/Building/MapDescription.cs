using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.ScenesManagement;
using UnityEngine;

namespace Scripts.Building
{
    public class MapDescription : ICloneable
    {
        public string MapName = "DefaultMapName";

        /// <summary>
        /// Position according to Layout array
        /// </summary>
        public Vector3Int StartGridPosition;

        public Quaternion PlayerRotation;
        public string SceneName;
        public TileDescription[,,] Layout;
        public List<PrefabConfiguration> PrefabConfigurations;

        public MapDescription()
        {
            PrefabConfigurations = new List<PrefabConfiguration>();
            StartGridPosition = Vector3Int.zero;
            PlayerRotation = Quaternion.identity;
            SceneName = Scenes.PlayIndoorSceneName;
        }

        public MapDescription ClonedCopy() => (MapDescription)Clone();
        
        public object Clone() => new MapDescription
        {
            MapName = MapName,
            StartGridPosition = StartGridPosition,
            PlayerRotation = PlayerRotation,
            SceneName = SceneName,
            Layout = (TileDescription[,,]) Layout.Clone(),
            PrefabConfigurations = PrefabConfigurations.Select(configuration => (PrefabConfiguration) configuration.Clone()).ToList()
        };
    }
}