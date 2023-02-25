using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsBuilding;
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
    /// TODO: Not sure how to go about replacing move walls with other kind of walls, maybe offer to delete waypoints on RemoveOtherComponents
    /// </summary>
    public class WallEditor : PrefabEditorBase<WallConfiguration, WallPrefabBase, WallService>
    {
        private LabeledSlider _offsetSlider;
        private NumericUpDown _offsetNumericUpDown;
        private WaypointEditor _waypointEditor;
        private Button _createOppositePathButton;

        private WallConfiguration _createdOppositeWall;

        protected override WallConfiguration GetNewConfiguration(string prefabName) => new()
        {
            Guid = Guid.NewGuid().ToString(),
            PrefabType = EditedPrefabType,
            PrefabName = prefabName,
            TransformData = new PositionRotation(SelectedCursor.transform.position, SelectedCursor.transform.rotation),
            SpawnPrefabOnBuild = true,

            WayPoints = new List<Waypoint>(),
            Offset = 0f,
        };

        protected override WallConfiguration CloneConfiguration(WallConfiguration sourceConfiguration) => new(sourceConfiguration);

        public override Vector3 GetCursor3DScale() => new(0.15f, 1.1f, 1.1f);

        public override void Open()
        {
            if(_waypointEditor) _waypointEditor.SetActive(false);
            
            base.Open();
        }

        protected override void Delete()
        {
            RemoveOtherComponents();

            base.Delete();
        }

        protected override void SaveMap()
        {
            if (EditedConfiguration.HasPath())
            {
                HighlightPath(EPathsType.Waypoint, EditedConfiguration.Guid, false);
            }

            if (_createdOppositeWall != null)
            {
                MapBuilder.AddReplacePrefabConfiguration(_createdOppositeWall);
            }

            _createdOppositeWall = null;

            base.SaveMap();
        }

        protected override void RemoveAndReopen()
        {
            if (IsCurrentConfigurationChanged)
            {
                if (EditedConfiguration?.WayPoints.Any() == true)
                {
                    DestroyPath(EPathsType.Waypoint, EditedConfiguration.Guid);
                }
                
                RemoveOtherComponents();
            }
            else
            {
                if (EditedConfiguration?.WayPoints.Any() == true)
                {
                    HighlightPath(EPathsType.Waypoint, EditedConfiguration.Guid, false);
                }
            }
            
            base.RemoveAndReopen();
        }

        private static PositionRotation CalculateWallForPath(List<Waypoint> path, bool isForLowerLadderPath)
        {
            if (path.Count < 1) return null;

            Vector3 startDirection = isForLowerLadderPath
                ? (path[^1].position.Round(1) - path[^2].position.Round(1)).normalized
                : (path[1].position.Round(1) - path[0].position.Round(1)).normalized;

            return !V3Extensions.WallDirectionRotationMap.ContainsKey(startDirection)
                ? null
                : new PositionRotation(
                    path[0].position.Round(1) + (startDirection * 0.5f),
                    V3Extensions.WallDirectionRotationMap[startDirection]
                );
        }

        protected override void RemoveOtherComponents()
        {
            if (EditedConfiguration != null && EditedConfiguration.WayPoints.Any())
            {
                DestroyPath(EPathsType.Waypoint, EditedConfiguration.Guid);
            }
            
            if (_createdOppositeWall != null)
            {
                MapBuilder.RemovePrefab(_createdOppositeWall);

                if (_createdOppositeWall.WayPoints.Any())
                {
                    DestroyPath(EPathsType.Waypoint, _createdOppositeWall.Guid);
                }
            }

            _createdOppositeWall = null;
        }

        protected override void InitializeOtherComponents()
        {
            _offsetSlider = Content.Find("OffsetHandling/OffsetSlider").GetComponent<LabeledSlider>();
            _offsetSlider.SetActive(false);
            _offsetSlider.SetLabel(t.Get(Keys.Offset));

            _offsetNumericUpDown = Content.Find("OffsetHandling/NumericUpDown").GetComponent<NumericUpDown>();
            _offsetNumericUpDown.gameObject.SetActive(false);

            _waypointEditor = body.transform.Find("WaypointsEditor").GetComponent<WaypointEditor>();
            _waypointEditor.SetActive(false);

            _createOppositePathButton = Content.Find("CreateOppositePathButton").GetComponent<Button>();
            _createOppositePathButton.onClick.RemoveAllListeners();
            _createOppositePathButton.onClick.AddListener(GenerateOppositePath);
            _createOppositePathButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.CreateOppositePath);

            _createOppositePathButton.gameObject.SetActive(false);
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

            MapBuilder.BuildPrefab(_createdOppositeWall, true);
            HighlightPath(EPathsType.Waypoint, _createdOppositeWall.Guid);
            
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

        protected override void VisualizeOtherComponents()
        {
            _waypointEditor.SetActive(false);
            _offsetSlider.SetCollapsed(true);
            _offsetNumericUpDown.SetCollapsed(true);
            _createOppositePathButton.gameObject.SetActive(false);

            if (EditedConfiguration == null) return;

            if (EditedConfiguration.SpawnPrefabOnBuild && PhysicalPrefabBody)
            {
                _offsetSlider.OnValueChanged.RemoveAllListeners();
                _offsetSlider.SetCollapsed(false);
                _offsetSlider.Value = EditedConfiguration.Offset;
                _offsetSlider.OnValueChanged.AddListener(OnOffsetValueChanged);

                _offsetNumericUpDown.OnValueChanged.RemoveAllListeners();
                _offsetNumericUpDown.SetCollapsed(false);
                _offsetNumericUpDown.Value = EditedConfiguration.Offset;
                _offsetNumericUpDown.minimum = -0.5f;
                _offsetNumericUpDown.maximum = 0.5f;
                _offsetNumericUpDown.step = 0.05f;
                _offsetNumericUpDown.OnValueChanged.AddListener(OnOffsetValueChanged);
            }

            if (!EditedPrefab) return;

            if (EditedPrefab.presentedInEditor)
            {
                EditedPrefab.transform.Find("EditorPresentation").gameObject.SetActive(true);
            }

            if (EditedPrefab is IMovementWall movementScript)
            {
                IEnumerable<Waypoint> waypoints = movementScript.GetWaypoints();
                
                if (EditedConfiguration.WayPoints.Count < 2 && waypoints?.Any() == true)
                {
                    EditedConfiguration.WayPoints = waypoints.ToList();
                }
                else if (EditedConfiguration.WayPoints.Count == 0)
                {
                    EditedConfiguration.WayPoints.Add(
                        new Waypoint(
                            EditorMouseService.Instance.LastLeftButtonUpWorldPosition,
                            0.3f));
                }

                if (EditedConfiguration.SpawnPrefabOnBuild)
                {
                    _waypointEditor.SetActive(true, EditedConfiguration.WayPoints, OnPathChanged);
                }
                
                AddReplaceWaypointPath(EditedConfiguration.Guid, EditedConfiguration.WayPoints, true);
                HandleCreateOppositePathButton();
            }
        }

        private void OnPathChanged(IEnumerable<Waypoint> path, int effectedWaypointIndex)
        {
            SetEdited();
            DestroyPath(EPathsType.Waypoint, EditedConfiguration.Guid);
            List<Waypoint> waypoints = path.ToList();
            EditedConfiguration.WayPoints = waypoints;
            AddReplaceWaypointPath(EditedConfiguration.Guid, waypoints, true);
            HighlightPoint(EPathsType.Waypoint, EditedConfiguration.Guid, effectedWaypointIndex, isExclusiveHighlight: true);
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