using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.MapEditor.Services;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EntryPointEditor : PrefabEditorBase<EntryPointConfiguration, EntryPointPrefab, EntryPointService>
    {
        protected override EntryPointConfiguration GetNewConfiguration(string prefabName)
        {
            throw new NotImplementedException();
        }

        protected override EntryPointConfiguration CloneConfiguration(EntryPointConfiguration sourceConfiguration)
        {
            throw new NotImplementedException();
        }

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