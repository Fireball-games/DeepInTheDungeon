using System;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Walls;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System;
using Scripts.UI.Components;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class PrefabTileEditor : PrefabEditorBase<TilePrefabConfiguration, TilePrefab>
    {
        [SerializeField] private RotationWidget rotationWidget;
        [SerializeField] private FramedCheckBox isWalkableCheckBox;

        protected override TilePrefabConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(Placeholder.transform.position, Quaternion.Euler(Vector3.zero)),
            SpawnPrefabOnBuild = true,
            
            IsWalkable = false,
        };

        protected override TilePrefabConfiguration CloneConfiguration(TilePrefabConfiguration sourceConfiguration) => new(sourceConfiguration);

        public override void Open(TilePrefabConfiguration configuration)
        {
            if (!CanOpen) return;

            if (configuration == null)
            {
                Close();
                return;
            }

            base.Open(configuration);

            TilePrefab script = PhysicalPrefabBody.GetComponentInParent<TilePrefab>();

            if (!PhysicalPrefabBody || !script)
            {
                Logger.Log($"loaded prefab {configuration.PrefabName} was either not loaded or missing {nameof(TilePrefab)} script.");
                return;
            }

            MapBuilder.Layout.ByGridV3Int(PhysicalPrefabBody.transform.position.ToGridPosition()).IsForMovement = script.isWalkable;
        }

        protected override void SetPrefab(string prefabName)
        {
            base.SetPrefab(prefabName);

            MapBuilder.Layout.ByGridV3Int(PhysicalPrefabBody.transform.position.ToGridPosition()).IsForMovement = EditedConfiguration.IsWalkable;
        }

        protected override void VisualizeOtherComponents()
        {
            rotationWidget.SetUp(t.Get(Keys.Rotate), () => Rotate(-90), () => Rotate(90));

            isWalkableCheckBox.SetLabel(t.Get(Keys.IsWalkable));
            isWalkableCheckBox.SetToggle(EditedConfiguration.IsWalkable);
            isWalkableCheckBox.OnValueChanged += SetIsWalkableInLayout;
        }

        protected override void InitializeOtherComponents()
        {
            
        }

        private void Rotate(float angles)
        {
            SetEdited();
            PhysicalPrefabBody.transform.Rotate(Vector3.up, angles);
            EditedConfiguration.TransformData.Rotation = PhysicalPrefabBody.transform.rotation;
        }

        private void SetIsWalkableInLayout(bool isWalkable)
        {
            SetEdited();
            EditedConfiguration.IsWalkable = isWalkable;
            MapBuilder.Layout.ByGridV3Int(PhysicalPrefabBody.transform.position.ToGridPosition()).IsForMovement = isWalkable;
        }
    }
}