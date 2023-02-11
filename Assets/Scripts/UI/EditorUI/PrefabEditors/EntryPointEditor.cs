using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Localization;
using Scripts.System;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EntryPointEditor : PrefabEditorBase<EntryPointConfiguration, EntryPointPrefab, EntryPointService>
    {
        private readonly Vector3 _defaultRotation = new(0, -90, 0);
        
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