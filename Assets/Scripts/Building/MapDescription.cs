using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<string> EntryPointsNames => EntryPoints.Select(e => e.name);
        
        public EntryPoint GetEntryPointByName(string entryPointName)
        {
            EntryPoint entryPoint = EntryPoints.FirstOrDefault(ep => ep.name == entryPointName);
            if (entryPoint != null) return entryPoint;
            
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
            EntryPoints = EntryPoints.Clone(),
        };
    }
}