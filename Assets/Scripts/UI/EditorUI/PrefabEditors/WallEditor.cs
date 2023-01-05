using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.PrefabsSpawning.Walls;
using Scripts.Building.PrefabsSpawning.Walls.Identifications;
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
using static Scripts.MapEditor.Services.PathsService;

namespace Scripts.UI.EditorUI
{
    /// <summary>
    /// PrefabEditorBase overload for building walls
    /// </summary>
    public class WallEditor : PrefabEditorBase<WallConfiguration, WallPrefabBase>
    {
        private LabeledSlider _offsetSlider;
        private NumericUpDown _offsetNumericUpDown;
        private WaypointEditor _waypointEditor;
        private Button _createOppositePathButton;

        private WallConfiguration _createdOppositeWall;

        protected override WallConfiguration GetNewConfiguration(string prefabName)
        {
            return new WallConfiguration
            {
                Guid = Guid.NewGuid().ToString(),
                PrefabType = EditedPrefabType,
                PrefabName = prefabName,
                TransformData = new PositionRotation(Placeholder.transform.position, Placeholder.transform.rotation),
                WayPoints = new List<Waypoint>(),
                Offset = 0f,
                SpawnPrefabOnBuild = true,
            };
        }

        protected override WallConfiguration CopyConfiguration(WallConfiguration sourceConfiguration) => new(EditedConfiguration);

        protected override Vector3 Cursor3DScale => new(0.15f, 1.1f, 1.1f);

        public override void Open(WallConfiguration configuration)
        {
            if (!CanOpen) return;

            if (configuration == null)
            {
                Close();
                return;
            }

            base.Open(configuration);

            VisualizeOtherComponents();
        }
        
        protected override string SetupWindow(EPrefabType prefabType, bool deleteButtonActive)
        {
            Initialize();

            _offsetSlider.SetLabel(t.Get(Keys.Offset));
            _offsetSlider.SetActive(false);
            _offsetNumericUpDown.gameObject.SetActive(false);
            
            _createOppositePathButton.gameObject.SetActive(false);

            return base.SetupWindow(prefabType, deleteButtonActive);
        }

        protected override void SetPrefab(string prefabName)
        {
            base.SetPrefab(prefabName);

            _waypointEditor.SetActive(false);

            VisualizeOtherComponents();
        }

        protected override void Delete()
        {
            if (EditedConfiguration.WayPoints.Any())
            {
                DestroyPath(EPathsType.Waypoint, EditedConfiguration.WayPoints);
            }

            RemoveExtraParts();

            base.Delete();
        }

        protected override void SaveMapAndClose()
        {
            if (EditedConfiguration.HasPath())
            {
                HighlightPath(EPathsType.Waypoint, EditedConfiguration.WayPoints, false);
            }

            if (_createdOppositeWall != null)
            {
                MapBuilder.AddReplacePrefabConfiguration(_createdOppositeWall);
            }

            _createdOppositeWall = null;

            base.SaveMapAndClose();
        }

        protected override void RemoveAndClose()
        {
            if (EditedConfiguration.WayPoints.Any())
            {
                DestroyPath(EPathsType.Waypoint, EditedConfiguration.WayPoints);
            }

            RemoveExtraParts();

            base.RemoveAndClose();
        }

        private static PositionRotation CalculateWallForPath(List<Waypoint> path, bool isForLowerLadderPath)
        {
            if (path.Count < 1) return null;

            Vector3 startDirection = isForLowerLadderPath 
                ? (path[^1].position.Round(1) - path[^2].position.Round(1)).normalized
                : (path[1].position.Round(1) - path[0].position.Round(1)).normalized;

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
                    DestroyPath(EPathsType.Waypoint, _createdOppositeWall.WayPoints);
                }
            }

            _createdOppositeWall = null;
        }

        private void Initialize()
        {
            _offsetSlider = body.transform.Find("Background/Frame/OffsetHandling/OffsetSlider").GetComponent<LabeledSlider>();
            _offsetNumericUpDown = body.transform.Find("Background/Frame/OffsetHandling/NumericUpDown").GetComponent<NumericUpDown>();
            _waypointEditor = body.transform.Find("WaypointsEditor").GetComponent<WaypointEditor>();
            _waypointEditor.SetActive(false);
            
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

            bool isForLadderDown = IsLadderDownAtPathStart(EditedConfiguration.WayPoints);
            
            string prefabName = isForLadderDown
                ? Vector3.Distance(oppositePoints[0].position, oppositePoints[1].position) > 1
                    ? "SteelLadderStart"
                    : "SteelLadderStartTop"
                : "MoveWall";
            
            PositionRotation transformData = isForLadderDown
                ? new PositionRotation(
                    wallTransformData.Position,
                    Quaternion.Euler(wallTransformData.Rotation.eulerAngles - new Vector3(0, 180, 0)))
                : wallTransformData;

            EPrefabType prefabType = isForLadderDown ? EPrefabType.WallOnWall : EPrefabType.WallBetween;
            
            _createdOppositeWall = new()
            {
                WayPoints = oppositePoints,
                TransformData = transformData,
                PrefabType = prefabType,
                PrefabName = prefabName,
                SpawnPrefabOnBuild = true,
                Guid = Guid.NewGuid().ToString(),
            };

            MapBuilder.BuildPrefab(_createdOppositeWall);
            AddWaypointPath(EPathsType.Waypoint, oppositePoints);
            SetEdited();
        }

        private void OnOffsetValueChanged(float value)
        {
            SetEdited();
            Vector3 newPosition = PhysicalPrefabBody.transform.localPosition;
            _offsetSlider.Value = value;
            _offsetNumericUpDown.Value = value;
            newPosition.x = value;
            EditedConfiguration.Offset = value;
            PhysicalPrefabBody.transform.localPosition = newPosition;
        }

        private void VisualizeOtherComponents()
        {
            if (PhysicalPrefabBody)
            {
                _offsetSlider.OnValueChanged.RemoveAllListeners();
                _offsetSlider.SetActive(true);
                _offsetSlider.Value = EditedConfiguration.Offset;
                _offsetSlider.OnValueChanged.AddListener(OnOffsetValueChanged);
                
                _offsetNumericUpDown.OnValueChanged.RemoveAllListeners();
                _offsetNumericUpDown.gameObject.SetActive(true);
                _offsetNumericUpDown.Value = EditedConfiguration.Offset;
                _offsetNumericUpDown.minimum = -0.5f;
                _offsetNumericUpDown.maximum = 0.5f;
                _offsetNumericUpDown.step = 0.05f;
                _offsetNumericUpDown.OnValueChanged.AddListener(OnOffsetValueChanged);
            }
            
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
                AddWaypointPath(EPathsType.Waypoint, EditedConfiguration.WayPoints, true);
                HandleCreateOppositePathButton();
            }
        }

        private void OnPathChanged(IEnumerable<Waypoint> path, int effectedWaypointIndex)
        {
            SetEdited();
            DestroyPath(EPathsType.Waypoint, EditedConfiguration.WayPoints);
            List<Waypoint> waypoints = path.ToList();
            EditedConfiguration.WayPoints = waypoints;
            AddWaypointPath(EPathsType.Waypoint, waypoints);
            HighlightPoint(EPathsType.Waypoint, waypoints, effectedWaypointIndex, isExclusiveHighlight: true);
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

            if (wallTransformData == null) return false;

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

            bool isUpperLadderPath = IsLadderDownAtPathStart(waypoints.ToList());

            wallTransformData = CalculateWallForPath(oppositePoints, isUpperLadderPath);
            return wallTransformData != null;
        }
    }
}