using System;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.System;
using Scripts.Triggers;
using Scripts.UI.Components;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class TilePrefabEditor : PrefabEditorBase<TilePrefabConfiguration, TilePrefab, TilePrefabService>
    {
        [SerializeField] private RotationWidget rotationWidget;
        [SerializeField] private FramedCheckBox isWalkableCheckBox;

        protected override TilePrefabConfiguration GetNewConfiguration(string prefabName)
        {
            TilePrefab storedPrefab = PrefabStore.GetPrefabByName<TilePrefab>(prefabName);
            
            return new TilePrefabConfiguration
            {
                Guid = Guid.NewGuid().ToString(),
                PrefabType = EditedPrefabType,
                PrefabName = prefabName,
                TransformData = new PositionRotation(SelectedCage.transform.position, Quaternion.Euler(Vector3.zero)),
                SpawnPrefabOnBuild = true,

                IsWalkable = storedPrefab.isWalkable,
            };
        }

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
            
            PhysicalPrefab.transform.Rotate(Vector3.up, angles);
            EditedConfiguration.TransformData.Rotation = PhysicalPrefab.transform.rotation;
            
            UpdateEmbeddedPrefabsTransformData();
        }

        private void UpdateEmbeddedPrefabsTransformData()
        {
            // TriggerReceivers are not PrefabBase so they need special handling.
            foreach (TriggerReceiver receiver in EditedPrefab.GetComponents<TriggerReceiver>())
            {
                TriggerReceiverConfiguration configuration = MapBuilder.GetConfigurationByGuid<TriggerReceiverConfiguration>(receiver.Guid);
                configuration.TransformData.Position = receiver.transform.position;
                configuration.TransformData.Rotation = receiver.transform.rotation;
            }
            
            UpdatePrefabTransformData<TriggerReceiverConfiguration, Trigger>();
            UpdatePrefabTransformData<WallConfiguration, WallPrefabBase>();
        }

        private void UpdatePrefabTransformData<TConfiguration, TPrefabType>() where TConfiguration : PrefabConfiguration where TPrefabType : PrefabBase
        {
            foreach (TPrefabType prefab in EditedPrefab.GetComponentsInChildren<TPrefabType>())
            {
                TConfiguration configuration = MapBuilder.GetConfigurationByGuid<TConfiguration>(prefab.Guid);

                configuration.TransformData.Position = prefab.transform.position;
                configuration.TransformData.Rotation = prefab.transform.rotation;
                
                if (configuration is WallConfiguration wallConfiguration 
                    && prefab is WallMovement movementPrefab 
                    && wallConfiguration.HasPath())
                {
                    wallConfiguration.WayPoints = movementPrefab.GetWaypoints().ToList();
                    PathsService.AddReplaceWaypointPath(wallConfiguration.Guid);
                }
                
                if (configuration is TriggerConfiguration triggerConfiguration) PathsService.AddReplaceTriggerPath(triggerConfiguration);
            }
        }

        private void SetIsWalkableInLayout(bool isWalkable)
        {
            SetEdited();
            EditedConfiguration.IsWalkable = isWalkable;
            MapBuilder.SetTileForMovement(PhysicalPrefabBody.transform.position, isWalkable);
        }
    }
}