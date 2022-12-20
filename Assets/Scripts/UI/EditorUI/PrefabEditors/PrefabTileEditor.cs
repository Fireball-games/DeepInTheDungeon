using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.System;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class PrefabTileEditor : PrefabEditorBase<TilePrefabConfiguration, TilePrefab>
    {
        protected override TilePrefabConfiguration GetNewConfiguration(string prefabName) => new()
        {
            IsWalkable = EditedConfiguration.IsWalkable,
            PrefabType = EditedPrefabType,
            PrefabName = AvailablePrefabs.FirstOrDefault(prefab => prefab.name == prefabName)?.name,
            TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
        };

        protected override TilePrefabConfiguration CopyConfiguration(TilePrefabConfiguration sourceConfiguration) => new(sourceConfiguration);
    }
}