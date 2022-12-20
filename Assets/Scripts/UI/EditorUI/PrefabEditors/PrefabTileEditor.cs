using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class PrefabTileEditor : PrefabEditorBase<TilePrefabConfiguration, TilePrefab>
    {
        protected override TilePrefabConfiguration GetNewConfiguration(string prefabName) => new()
        {
            IsWalkable = false,
            PrefabType = EditedPrefabType,
            PrefabName = AvailablePrefabs.FirstOrDefault(prefab => prefab.name == prefabName)?.name,
            TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
        };

        protected override TilePrefabConfiguration CopyConfiguration(TilePrefabConfiguration sourceConfiguration) => new(sourceConfiguration);
        
        public override void Open(TilePrefabConfiguration configuration)
        {
            if (!CanOpen) return;
            
            if (configuration == null)
            {
                Close();
                return;
            }
            
            base.Open(configuration);
            
            PhysicalPrefab = MapBuilder.GetPrefabByConfiguration(configuration);
            TilePrefab script = PhysicalPrefab.GetComponent<TilePrefab>();

            if (!PhysicalPrefab || !script)
            {
                Logger.Log($"loaded prefab {configuration.PrefabName} was either not loaded or missing {nameof(TilePrefab)} script.");
                return;
            }

            MapBuilder.Layout.ByGridV3Int(PhysicalPrefab.transform.position.ToGridPosition()).IsForMovement = script.isWalkable;
        }

        protected override void SetPrefab(string prefabName)
        {
            base.SetPrefab(prefabName);
            
            MapBuilder.Layout.ByGridV3Int(PhysicalPrefab.transform.position.ToGridPosition()).IsForMovement = false;
        }
    }
}