using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.ItemSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using Scripts.ScenesManagement;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building
{
    public class MapDescription : ICloneable
    {
        public string MapName = "DefaultMapName";

        /// <summary>
        /// Position according to Layout array
        /// </summary>
        public Vector3Int EditorStartPosition = Vector3Int.zero;

        public Quaternion EditorPlayerStartRotation = Quaternion.identity;
        public string SceneName = Scenes.PlayIndoorSceneName;
        public TileDescription[,,] Layout = new TileDescription[10, 10, 1];
        public List<PrefabConfiguration> PrefabConfigurations = new();
        public List<MapObjectConfiguration> MapObjects = new();
        public List<EntryPoint> EntryPoints = new();

        public bool IsOutdoor;
        public int groundIndex;
        public string MusicTrackName;

        public MapDescription ClonedCopy() => (MapDescription) Clone();

        public IEnumerable<string> EntryPointsNames => EntryPoints.Select(e => e.name);
        
        public EntryPoint GetEntryPointCloneByName(string entryPointName)
        {
            EntryPoint entryPoint = EntryPoints.FirstOrDefault(ep => ep.name == entryPointName);
            if (entryPoint != null) return entryPoint.Cloned();
            
            Logger.LogWarning($"Entry point not found in map: {MapName}, entry point name: {entryPointName}");
            
            return null;
        }

        public IEnumerable<TriggerConfiguration> CollectMapTraversalTriggers()
            => PrefabConfigurations.OfType<TriggerConfiguration>().Where(c => !string.IsNullOrEmpty(c.TargetMapName));

        public object Clone() => new MapDescription
        {
            EditorStartPosition = EditorStartPosition,
            EditorPlayerStartRotation = EditorPlayerStartRotation,
            SceneName = SceneName,
            MapName = MapName,
            Layout = GeneralExtensions.Clone(Layout),
            PrefabConfigurations = PrefabConfigurations.Clone(),
            MapObjects = MapObjects.Clone(),
            EntryPoints = EntryPoints.Clone(),
            IsOutdoor = false,
            MusicTrackName = string.Empty,
        };
    }
}