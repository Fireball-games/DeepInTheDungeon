using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EntryPointEditor : PrefabEditorBase<EntryPointConfiguration, EntryPointPrefab, EntryPointService>
    {
        
        
        protected override IEnumerable<EntryPointConfiguration> GetExistingConfigurations() 
            => MapBuilder.MapDescription.EntryPoints.Select(ep => ep.ToConfiguration());

        protected override EntryPointConfiguration GetNewConfiguration(string prefabName) => new();

        protected override EntryPointConfiguration CloneConfiguration(EntryPointConfiguration sourceConfiguration) =>
            new(sourceConfiguration);

        protected override void VisualizeOtherComponents()
        {
            Logger.LogNotImplemented();
        }

        protected override void InitializeOtherComponents()
        {
            Logger.LogNotImplemented();
        }

        protected override void RemoveOtherComponents()
        {
            Logger.LogNotImplemented();
        }
    }
}