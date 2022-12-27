using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.PrefabsSpawning.Walls.Indentificators;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.ScriptableObjects;
using Scripts.System;
using Scripts.UI.Components;
using Scripts.UI.EditorUI.PrefabEditors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.Enums;

namespace Scripts.UI.EditorUI
{
    /// <summary>
    /// PrefabEditorBase overload for building walls
    /// Limitations:
    /// - Cant create opposite path for upper ladder part, respectively, you can, but it wont generate correctly
    /// </summary>
    public class WallEditor : PrefabEditorBase<WallConfiguration, WallPrefabBase>
    {
        private LabeledSlider _offsetSlider;
        private WaypointEditor _waypointEditor;
        private Button _createOppositePathButton;

        private WallConfiguration _createdOppositeWall;

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

            RemoveExtraParts();

            base.Delete();
        }

        protected override void SaveMapAndClose()
        {
            if (EditedConfiguration.HasPath())
            {
                WayPointService.HighlightPath(EditedConfiguration.WayPoints, false);
            }

            if (_createdOppositeWall != null)
            {
                MapBuilder.AddReplacePrefabConfiguration(_createdOppositeWall);
            }

            base.SaveMapAndClose();
        }

        protected override void RemoveAndClose()
        {
            if (EditedConfiguration.WayPoints.Any())
            {
                WayPointService.DestroyPath(EditedConfiguration.WayPoints);
            }

            RemoveExtraParts();

            base.RemoveAndClose();
        }

        public override void CloseWithChangeCheck()
        {
            RemoveExtraParts();

            base.CloseWithChangeCheck();
        }

        private static PositionRotation CalculateWallForPath(List<Waypoint> path)
        {
            if (path.Count < 1) return null;

            Vector3 startDirection = (path[1].position.Round(1) - path[0].position.Round(1)).normalized;

            return !V3Extensions.WallDirectionRotationMap.ContainsKey(startDirection) 
                ? null : 
                new PositionRotation(
                    path[0].position.Round(1) + (startDirection * 0.5f),
                    V3Extensions.WallDirectionRotationMap[startDirection]
                    );
        }

        private void RemoveExtraParts()
        {
            if (_createdOppositeWall != null)
            {
                MapBuilder.RemovePrefab(_createdOppositeWall);

                if (_createdOppositeWall.WayPoints.Any())
                {
                    WayPointService.DestroyPath(_createdOppositeWall.WayPoints);
                }
            }

            _createdOppositeWall = null;
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
            if (!CalculateOppositePath(EditedConfiguration.WayPoints, out PositionRotation wallTransformData,
                    out List<Waypoint> oppositePoints))
            {
                return;
            }

            // Cant create opposite path for upper ladder part
            _createdOppositeWall = new()
            {
                WayPoints = oppositePoints,
                TransformData = wallTransformData,
                PrefabType = EPrefabType.WallBetween,
                PrefabName = "MoveWall",
            };

            MapBuilder.BuildPrefab(_createdOppositeWall);
            WayPointService.AddPath(oppositePoints);
            SetEdited();
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

            if (script is IMovementWall movementScript)
            {
                if (EditedConfiguration.WayPoints.Count < 2 && movementScript.GetWaypointPreset())
                {
                    List<Waypoint> translatedWaypoints = new();

                    foreach (Waypoint waypoint in movementScript.GetWaypointPreset().waypoints)
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
                WayPointService.AddPath(EditedConfiguration.WayPoints, true);
                EditorCameraService.Instance.ResetCamera();
                HandleCreateOppositePathButton();
            }
        }

        private void OnPathChanged(IEnumerable<Waypoint> path)
        {
            SetEdited();
            WayPointService.DestroyPath(EditedConfiguration.WayPoints);
            List<Waypoint> waypoints = path.ToList();
            EditedConfiguration.WayPoints = waypoints;
            WayPointService.AddPath(waypoints, true);
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
                .Select(c => c as WallConfiguration)
                .ToList();

            if (!walls.Any()) return false;

            if (!CalculateOppositePath(EditedConfiguration.WayPoints, out PositionRotation wallTransformData,
                    out List<Waypoint> _))
            {
                return false;
            }

            return walls.FirstOrDefault(w => w.TransformData.Position == wallTransformData.Position) != null;
        }

        /// <summary>
        /// calculates data for path in opposite direction.
        /// </summary>
        /// <param name="waypoints">Waypoints to calculate opposite path from.</param>
        /// <param name="wallTransformData">Returned PositionRotation data for the wall in opposite direction.</param>
        /// <param name="oppositePoints">Returned waypoints for path in opposite direction.</param>
        /// <returns>True if path and wall data could be calculated.</returns>
        private bool CalculateOppositePath(IEnumerable<Waypoint> waypoints, out PositionRotation wallTransformData, out List<Waypoint> oppositePoints)
        {
            if (waypoints.Count() < 2)
            {
                wallTransformData = null;
                oppositePoints = null;
                return false;
            }

            oppositePoints = new List<Waypoint>();

            for (int i = waypoints.Count() - 1; i >= 0; i--)
            {
                oppositePoints.Add(new Waypoint(waypoints.ElementAt(i).position.Round(2), waypoints.ElementAt(i).moveSpeedModifier));
            }

            wallTransformData = CalculateWallForPath(oppositePoints);
            return wallTransformData != null;
        }
    }
}