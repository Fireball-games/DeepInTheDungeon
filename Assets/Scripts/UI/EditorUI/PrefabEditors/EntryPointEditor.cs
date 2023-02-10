using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;

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
            throw new NotImplementedException();
        }

        protected override void InitializeOtherComponents()
        {
            throw new NotImplementedException();
        }

        protected override void RemoveOtherComponents()
        {
            throw new NotImplementedException();
        }
    }
}