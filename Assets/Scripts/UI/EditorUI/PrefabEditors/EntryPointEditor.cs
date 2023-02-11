using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Localization;
using Scripts.System;
using Scripts.UI.Components;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EntryPointEditor : PrefabEditorBase<EntryPointConfiguration, EntryPointPrefab, EntryPointService>
    {
        [ConfigurableProperty(nameof(EntryPointConfiguration.IsMovingForwardOnStart), Keys.MoveOnEnter , nameof(OnIsMovingForwardOnStartChanged))]
        private FramedCheckBox _isMovingForwardOnStartCheckBox;
        
        [ConfigurableProperty(nameof(EntryPointConfiguration.EntryPointName), Keys.EnterName, nameof(OnEntryPointNameChanged))]
        private InputField _nameInputField;
        
        protected override IEnumerable<EntryPointConfiguration> GetExistingConfigurations() 
            => MapBuilder.MapDescription.EntryPoints.Select(ep => ep.ToConfiguration());

        protected override EntryPointConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(SelectedCage.transform.position, Quaternion.Euler(Vector3.zero)),
            SpawnPrefabOnBuild = true,

            IsMovingForwardOnStart = true,
            EntryPointName = t.Get(Keys.EnterName),
            LookDirection = Quaternion.identity,
        };

        protected override EntryPointConfiguration CloneConfiguration(EntryPointConfiguration sourceConfiguration) =>
            new(sourceConfiguration);

        protected override void InitializeOtherComponents()
        {
            // Delete once generic editor is implemented.
        }

        protected override void RemoveOtherComponents()
        {
            // Nothing to do.
        }
        
        private void OnIsMovingForwardOnStartChanged(bool isMovingForwardOnStart)
        {
            Logger.Log($"Eureka! Received value is {isMovingForwardOnStart}");
        }
        
        private void OnEntryPointNameChanged(string entryPointName)
        {
            Logger.Log($"Eureka! Received value is {entryPointName}");
        }
    }
}