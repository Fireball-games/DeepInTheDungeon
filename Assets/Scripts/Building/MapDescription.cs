using System;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
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
        public Vector3Int EditorStartPosition;
        public Quaternion EditorPlayerStartRotation;
        public string SceneName;
        public TileDescription[,,] Layout;
        public List<PrefabConfiguration> PrefabConfigurations;
        public List<EntryPoint> EntryPoints;

        public MapDescription()
        {
            EditorStartPosition = Vector3Int.zero;
            EditorPlayerStartRotation = Quaternion.identity;
            SceneName = Scenes.PlayIndoorSceneName;
            PrefabConfigurations = new List<PrefabConfiguration>();
            EntryPoints = new List<EntryPoint>();
        }

        public MapDescription ClonedCopy() => (MapDescription) Clone();

        public object Clone() => new MapDescription
        {
            EditorStartPosition = EditorStartPosition,
            EditorPlayerStartRotation = EditorPlayerStartRotation,
            SceneName = SceneName,
            MapName = MapName,
            Layout = GeneralExtensions.Clone(Layout),
            PrefabConfigurations = PrefabConfigurations.Clone(),
            EntryPoints = EntryPoints.Clone(),
        };
    }
}