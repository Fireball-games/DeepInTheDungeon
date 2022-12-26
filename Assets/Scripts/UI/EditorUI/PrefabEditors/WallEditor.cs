using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.ScriptableObjects;
using Scripts.System;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.PrefabEditors;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Scripts.Enums;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.UI.EditorUI
{
    public class WallEditor : PrefabEditorBase<WallConfiguration, WallPrefabBase>
    {
        private LabeledSlider _offsetSlider;
        private WaypointEditor _waypointEditor;
        private Button _createOppositePathButton;

        protected override WallConfiguration GetNewConfiguration(string prefabName)
        {
            return new WallConfiguration
            {
                PrefabType = EditedPrefabType,
                PrefabName = AvailablePrefabs.FirstOrDefault(prefab => prefab.name == prefabName)?.name,
                TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
                WayPoints = new List<Waypoint>(),
                Offset = 0f
            };
        }

        protected override WallConfiguration CopyConfiguration(WallConfiguration sourceConfiguration) => new(EditedConfiguration);

        protected override Vector3 Cursor3DScale => new(0.15f, 1f, 1f);

        public override void Open(WallConfiguration configuration)
        {
            if (!CanOpen) return;
            
            if (configuration == null)
            {
                Close();
                return;
            }
            
            base.Open(configuration);

            if (PhysicalPrefabBody)
            {
                _offsetSlider.SetActive(true);
                _offsetSlider.Value = configuration.Offset;
                _offsetSlider.slider.onValueChanged.RemoveAllListeners();
                _offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
            }

            VisualizeOtherComponents();
        }

        protected override string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            Initialize();
            
            _offsetSlider.SetLabel(t.Get(Keys.Offset));
            _offsetSlider.SetActive(false);
            
            return base.SetupWindow(prefabType, deleteButtonActive);
        }

        protected override void SetPrefab(string prefabName)
        {
            base.SetPrefab(prefabName);
            
            _waypointEditor.SetActive(false);
            
            if (PhysicalPrefabBody)
            {
                _offsetSlider.SetActive(true);
                _offsetSlider.Value = EditedConfiguration.Offset;
                _offsetSlider.slider.onValueChanged.AddListener(OnOffsetSliderValueChanged);
            }
            
            VisualizeOtherComponents();
        }

        protected override void Delete()
        {
            if (EditedConfiguration.WayPoints.Any())
            {
                WayPointService.DestroyPath(EditedConfiguration.WayPoints);
            }
            
            base.Delete();
        }

        protected override void SaveMapAndClose()
        {
            WayPointService.HighlightPath(EditedConfiguration.WayPoints, false);
            base.SaveMapAndClose();
        }

        protected override void RemoveAndClose()
        {
            if (EditedConfiguration.WayPoints.Any())
            {
                WayPointService.DestroyPath(EditedConfiguration.WayPoints);
            }
            
            base.RemoveAndClose();
        }

        private void Initialize()
        {
            _offsetSlider = body.transform.Find("Background/Frame/OffsetSlider").GetComponent<LabeledSlider>();
            _waypointEditor = body.transform.Find("WaypointsEditor").GetComponent<WaypointEditor>();
            _createOppositePathButton = body.transform.Find("Background/Frame/Buttons/CreateOppositePathButton").GetComponent<Button>();
            _createOppositePathButton.onClick.RemoveAllListeners();
            _createOppositePathButton.onClick.AddListener(GenerateOppositePath);
            _createOppositePathButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.CreateOppositePath);
        }

        private void GenerateOppositePath()
        {
            if (!CalculateOppositePath(EditedConfiguration.WayPoints, out Vector3 wallPosition,
                    out List<Waypoint> oppositePoints))
            {
                return;
            }
            
            
        }

        private void OnOffsetSliderValueChanged(float value)
        {
            SetEdited();
            Vector3 newPosition = PhysicalPrefabBody.transform.localPosition;
            newPosition.x = value;
            EditedConfiguration.Offset = value;
            PhysicalPrefabBody.transform.localPosition = newPosition;
        }

        private void VisualizeOtherComponents()
        {
            WallPrefabBase script = PhysicalPrefab.GetComponentInParent<WallPrefabBase>();

            if (!script) return;
            
            if (script.presentedInEditor)
            {
                script.transform.Find("EditorPresentation").gameObject.SetActive(true);
            }

            if (script is WallMovementBetween movementScript)
            {
                if (EditedConfiguration.WayPoints.Count < 2 && movementScript.waypointsPreset)
                {
                    List<Waypoint> translatedWaypoints = new();
                    
                    foreach (Waypoint waypoint in movementScript.waypointsPreset.waypoints)
                    {
                        Waypoint newWaypoint = new()
                        {
                            position = EditedConfiguration.TransformData.Position + waypoint.position,
                            moveSpeedModifier = waypoint.moveSpeedModifier
                        };
                        translatedWaypoints.Add(newWaypoint);
                    }
                    
                    EditedConfiguration.WayPoints = translatedWaypoints;
                }
                else if (EditedConfiguration.WayPoints.Count == 0)
                {
                    EditedConfiguration.WayPoints.Add(
                        new Waypoint(
                            EditorMouseService.Instance.LastLeftButtonUpWorldPosition,
                            0.3f));
                }

                _waypointEditor.SetActive(true, EditedConfiguration.WayPoints, OnPathChanged);
                WayPointService.AddPath(EditedConfiguration.WayPoints,true);
                EditorCameraService.Instance.ResetCamera();
                HandleCreateOppositePathButton();
            }
        }

        private void OnPathChanged(IEnumerable<Waypoint> path)
         { 
             SetEdited();
            WayPointService.DestroyPath(EditedConfiguration.WayPoints);
            EditedConfiguration.WayPoints = path.ToList();
            WayPointService.AddPath(path, true);
            HandleCreateOppositePathButton();
        }

        private void HandleCreateOppositePathButton()
        {
            _createOppositePathButton.gameObject.SetActive(EditedConfiguration.WayPoints.Count >= 2 && !IsPathInOppositeDirection());
        }

        private bool IsPathInOppositeDirection()
        {
            List<WallConfiguration> walls = MapBuilder.MapDescription.PrefabConfigurations
                .Where(c => c is WallConfiguration)
                .Select(c => c as WallConfiguration)?
                .ToList();

            if (!walls.Any()) return false;

            if (!CalculateOppositePath(EditedConfiguration.WayPoints, out Vector3 wallPosition,
                    out List<Waypoint> oppositePoints))
            {
                return false;
            }

            return walls.FirstOrDefault(w => w.TransformData.Position == wallPosition) != null;
        }

        private bool CalculateOppositePath(IEnumerable<Waypoint> waypoints, out Vector3 wallPosition, out List<Waypoint> oppositePoints)
        {
            if (waypoints.Count() < 2)
            {
                wallPosition = Vector3.zero;
                oppositePoints = null;
                return false;
            }

            oppositePoints = new List<Waypoint>();

            for (int i = waypoints.Count() - 1; i >= 0; i--)
            {
                oppositePoints.Add(new Waypoint(waypoints.ElementAt(i).position.Round(2), waypoints.ElementAt(i).moveSpeedModifier));
            }
            
            wallPosition = CalculateWallForPath(oppositePoints).Position;
            return true;
        }

        private PositionRotation CalculateWallForPath(List<Waypoint> path)
        {
            if (path.Count < 2) return null;
            
            Vector3 startDirection = (path[1].position.Round(1) - path[0].position.Round(1)).normalized;
            return  new PositionRotation(path[0].position.Round(1) + (startDirection * 0.5f), V3Extensions.DirectionRotationMap[startDirection]);
        }
    }
}