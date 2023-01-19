using System;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
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
            Placeholder.transform.position = SelectedCage.transform.position;
            
            base.SetPrefab(prefabName);

            TilePrefab prefabScript = PhysicalPrefab.GetComponent<TilePrefab>();
            MapBuilder.Layout.ByGridV3Int(PhysicalPrefab.transform.position.ToGridPosition()).IsForMovement = EditedConfiguration.IsWalkable;
            SetIsWalkableInLayout(prefabScript.isWalkable);

            if (prefabScript.disableFloor) WallService.ActivateWall(prefabScript.transform.position, TileDescription.ETileDirection.Floor, false);
            if (prefabScript.disableCeiling) WallService.ActivateWall(prefabScript.transform.position, TileDescription.ETileDirection.Ceiling, false);
        }

        protected override void VisualizeOtherComponents()
        {
            rotationWidget.SetActive(false);
            isWalkableCheckBox.SetActive(false);
            
            if (EditedConfiguration == null) return;
            
            rotationWidget.SetUp(t.Get(Keys.Rotate), () => Rotate(-90), () => Rotate(90));
            isWalkableCheckBox.SetActive(true, t.Get(Keys.IsWalkable), EditedConfiguration.IsWalkable);
        }

        protected override void InitializeOtherComponents()
        {
            isWalkableCheckBox.OnValueChanged += SetIsWalkableInLayout;
        }

        protected override void RemoveOtherComponents()
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