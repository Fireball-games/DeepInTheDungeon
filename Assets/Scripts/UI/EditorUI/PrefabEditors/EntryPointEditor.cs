using System;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Attributes;
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
            nameof(EntryPointConfiguration.Name),
            Keys.EnterName,
            nameof(OnEntryPointNameChanged))]
        private InputField _nameInputField;
        
        [ConfigurableProperty(
            nameof(EntryPointConfiguration.PlayerRotationY),
            Keys.Rotate,
            nameof(OnLookDirectionChanged),
            setValueFromConfiguration: false)]
        private RotationWidget _;

        protected override EntryPointConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(SelectedCursor.transform.position, Quaternion.Euler(Vector3.zero)),
            SpawnPrefabOnBuild = true,

            IsMovingForwardOnStart = true,
            Name = t.Get(Keys.NoNameSet),
            PlayerRotationY = 0,
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
                PhysicalPrefabBody.transform.rotation = Quaternion.Euler(0f, EditedConfiguration.PlayerRotationY, 0f);
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
            EditedConfiguration.Name = entryPointName;
        }
        
        private void OnLookDirectionChanged(int rotateDirection)
        {
            PhysicalPrefabBody.transform.rotation *= Quaternion.Euler(0f, rotateDirection * 90, 0f);
            EditedConfiguration.PlayerRotationY = (int)PhysicalPrefabBody.transform.rotation.eulerAngles.y;
        }
    }
}