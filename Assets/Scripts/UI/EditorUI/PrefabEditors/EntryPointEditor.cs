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

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class EntryPointEditor : PrefabEditorBase<EntryPointConfiguration, EntryPointPrefab, EntryPointService>
    {
        [ConfigurableProperty(
            nameof(EntryPointConfiguration.IsMovingForwardOnStart),
            Keys.MoveOnEnter,
            nameof(OnIsMovingForwardOnStartChanged))]
        private FramedCheckBox _isMovingForwardOnStartCheckBox;
        
        [ConfigurableProperty(
            nameof(EntryPointConfiguration.EntryPointName),
            Keys.EnterName,
            nameof(OnEntryPointNameChanged))]
        private InputField _nameInputField;
        
        [ConfigurableProperty(
            nameof(EntryPointConfiguration.LookDirection),
            Keys.Rotate,
            nameof(OnLookDirectionChanged),
            setValueFromConfiguration: false)]
        private RotationWidget _;

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
            // Nothing to do.
        }

        protected override void VisualizeOtherComponents()
        {
            base.VisualizeOtherComponents();

            if (PhysicalPrefabBody && EditedConfiguration != null)
            {
                PhysicalPrefabBody.transform.rotation = EditedConfiguration.LookDirection;
            }
        }

        protected override void RemoveOtherComponents()
        {
            // Nothing to do.
        }
        
        private void OnIsMovingForwardOnStartChanged(bool isMovingForwardOnStart)
        {
            EditedConfiguration.IsMovingForwardOnStart = isMovingForwardOnStart;
        }
        
        private void OnEntryPointNameChanged(string entryPointName)
        {
            EditedConfiguration.EntryPointName = entryPointName;
        }
        
        private void OnLookDirectionChanged(int lookDirection)
        {
            float currentRotationY = EditedConfiguration.LookDirection.eulerAngles.y;
            EditedConfiguration.LookDirection = Quaternion.Euler(0, currentRotationY + (lookDirection > 0 ? 90 : -90), 0);
            PhysicalPrefabBody.transform.rotation = EditedConfiguration.LookDirection;
        }
    }
}