using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using Scripts.System;
using UnityEngine;
using UnityEngine.Rendering;
using PathsType = System.Collections.Generic.Dictionary<Scripts.MapEditor.Services.PathsService.EPathsType, System.Collections.Generic.Dictionary<string, Scripts.MapEditor.PathController>>;

namespace Scripts.MapEditor.Services
{
    public class PathsService : MonoBehaviour
    {
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightedMaterial;
        [SerializeField] private Material waypointLinesNormalMaterial;
        [SerializeField] private Material waypointLinesHighlightedMaterial;
        [SerializeField] private Material triggerLinesNormalMaterial;
        [SerializeField] private Material triggerLinesHighlightedMaterial;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private Vector3 waypointScale = new(0.3f, 0.3f, 0.3f);

        private static PathsType _paths;
        private static Mesh _waypointMesh;
        private static Vector3 _waypointScale;
        private static Material _normalMaterial;
        private static Material _highlightedMaterial;
        private static Material _waypointsLineNormalMaterial;
        private static Material _waypointLinesHighlightedMaterial;
        private static Material _triggersLineNormalMaterial;
        private static Material _triggersLinesHighlightedMaterial;
        private static GameObject _parent;

        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;

        private static Dictionary<EPathsType, bool> _visibilityMap;
        private static Dictionary<EPathsType, Material> _lineNormalMaterialMap;
        private static Dictionary<EPathsType, Material> _lineHighlightedMaterialMap;

        public enum EPathsType
        {
            Waypoint,
            Trigger
        }

        private void Awake()
        {
            _paths = new PathsType
            {
                [EPathsType.Waypoint] = new(),
                [EPathsType.Trigger] = new()
            };

            _visibilityMap = new Dictionary<EPathsType, bool>
            {
                { EPathsType.Waypoint, true },
                { EPathsType.Trigger, false },
            };

            _parent = new GameObject("Waypoints");
            _waypointMesh = waypointPrefab.GetComponent<MeshFilter>().sharedMesh;
            _waypointScale = waypointScale;
            _normalMaterial = normalMaterial;
            _highlightedMaterial = highlightedMaterial;
            _waypointsLineNormalMaterial = waypointLinesNormalMaterial;
            _waypointLinesHighlightedMaterial = waypointLinesHighlightedMaterial;
            _triggersLineNormalMaterial = triggerLinesNormalMaterial;
            _triggersLinesHighlightedMaterial = triggerLinesHighlightedMaterial;
            
            _lineNormalMaterialMap = new Dictionary<EPathsType, Material>
            {
                {EPathsType.Waypoint, _waypointsLineNormalMaterial},
                {EPathsType.Trigger, _triggersLineNormalMaterial}
            };
            
            _lineHighlightedMaterialMap = new Dictionary<EPathsType, Material>
            {
                {EPathsType.Waypoint, _waypointLinesHighlightedMaterial},
                {EPathsType.Trigger, _triggersLinesHighlightedMaterial}
            };
        }

        public static void ShowPaths(EPathsType pathsType)
        {
            _visibilityMap[pathsType] = true;
            foreach (PathController controller in _paths[pathsType].Values)
            {
                controller.gameObject.SetActive(true);
            }
        }

        public static void HidePaths(EPathsType pathsType)
        {
            _visibilityMap[pathsType] = false;
            
            foreach (PathController controller in _paths[pathsType].Values.Where(controller => !controller.isHighlighted))
            {
                controller.gameObject.SetActive(false);
            }
        }

        public static void AddReplaceWaypointPath(string guid) =>
            AddReplaceWaypointPath(guid, MapBuilder.GetConfigurationByGuid<WallConfiguration>(guid).WayPoints);

        public static void AddReplaceWaypointPath(string guid, List<Waypoint> waypoints, bool highlightAfterBuild = false)
        {
            DestroyPath(EPathsType.Waypoint, guid);

            GameObject pathParent = new($"waypointPath_{waypoints[0].position}")
            {
                transform =
                {
                    parent = _parent.transform
                }
            };

            PathController controller = pathParent.AddComponent<PathController>();
            controller.typeOfPath = EPathsType.Waypoint;

            foreach (Waypoint waypoint in waypoints)
            {
                GameObject drawnWaypoint = DrawWaypoint(waypoint.position, controller);
                drawnWaypoint.transform.parent = pathParent.transform;
            }

            _paths[EPathsType.Waypoint].Add(guid, controller);

            BuildLines(controller);

            if (highlightAfterBuild)
            {
                HighlightPath(EPathsType.Waypoint, controller);
                return;
            }
            
            if (!_visibilityMap[EPathsType.Waypoint])
                HidePaths(EPathsType.Waypoint);
        }
        
        public static void AddReplaceTriggerPath(TriggerConfiguration configuration, bool highlightAfterBuild = false)
        {
            if (configuration.Subscribers.Count == 0)
            {
                DestroyPath(EPathsType.Trigger, configuration.Guid);
                return;
            }

            string key = configuration.Guid;
            
            List<Vector3> points = new() {configuration.TransformData.Position};
            points.AddRange(configuration.Subscribers
                .Where(s => !string.IsNullOrEmpty(s))
                // can't use TriggerService.TriggerReceivers here because it's not initialized yet on first load
                .Select(s => MapBuilder.GetConfigurationByGuid<TriggerReceiverConfiguration>(s))
                .Select(trc => trc.TransformData.Position.Round(2))
                .ToList());
            points[0] = points[0].Round(2);
            
            DestroyPath(EPathsType.Trigger, key);

            GameObject pathParent = new($"TriggerPath_{points[0]}")
            {
                transform =
                {
                    parent = _parent.transform
                }
            };

            PathController controller = pathParent.AddComponent<PathController>();
            controller.typeOfPath = EPathsType.Trigger;

            foreach (Vector3 point in points)
            {
                GameObject drawnWaypoint = DrawWaypoint(point, controller);
                drawnWaypoint.transform.parent = pathParent.transform;
            }

            _paths[EPathsType.Trigger].Add(key, controller);

            BuildLines(controller);

            if (highlightAfterBuild)
            {
                HighlightPath(EPathsType.Trigger, controller);
                return;
            }
            
            if (!_visibilityMap[EPathsType.Trigger])
                HidePaths(EPathsType.Trigger);
        }
        
        public static void DestroyAllPaths()
        {
            foreach (EPathsType pathType in new PathsType(_paths).Keys)
            {
                foreach (string key in new Dictionary<string, PathController>(_paths[pathType]).Keys)
                {
                    DestroyPath(pathType, key);
                }
            }
        }

        public static bool IsLadderDownAtPathStart(List<Waypoint> path)
        {
            if (path.Count < 3) return false;
            
            return (path[2].position.Round(1) - path[1].position.Round(1)).normalized == Vector3.down;
        }
        
        public static void HighlightPath(EPathsType pathType, string key, bool isHighlighted = true)
        {
            if (!_paths[pathType].TryGetValue(key, out PathController controller) || controller.isHighlighted == isHighlighted) return;

            controller.isHighlighted = isHighlighted;

            for (int index = 0; index < controller.Waypoints.Count; index++)
            {
                HighlightPoint(pathType, key, index, isHighlighted);
            }
            
            if (!isHighlighted && !_visibilityMap[pathType])
            {
                HidePaths(pathType);
            }
        }
        
        public static void HighlightPoint(EPathsType pathType, string key, int pointIndex, bool isHighlighted = true, bool isExclusiveHighlight = false)
        {
            Dictionary<GameObject, WaypointParts> waypointsParts = _paths[pathType][key].Waypoints;
            
            if (isExclusiveHighlight)
            {
                for (int index = 0; index < waypointsParts.Count; index++)
                {
                    HighlightPoint(pathType, waypointsParts.ElementAt(pointIndex).Value, index == pointIndex);
                }
            }
            
            HighlightPoint(pathType, waypointsParts.ElementAt(pointIndex).Value, isHighlighted);
        }
        
        public static void DestroyPath(EPathsType pathType, string key)
        {
            if (!_paths[pathType].ContainsKey(key)) return;

            Destroy(_paths[pathType][key].gameObject);
            _paths[pathType].Remove(key);
        }

        private static void HighlightPath(EPathsType pathType, PathController pathController, bool isHighlighted = true)
        {
            HighlightPath(pathType, _paths[pathType].GetFirstKeyByValue(pathController), isHighlighted);
        }

        private static void HighlightPoint(EPathsType pathType, WaypointParts waypoint, bool isHighlighted = true)
        {
            Material pointMaterial = isHighlighted ? _highlightedMaterial : _normalMaterial;
            Material lineMaterial = isHighlighted ? _lineHighlightedMaterialMap[pathType] : _lineNormalMaterialMap[pathType];

            waypoint.MeshRenderer.material = new Material(pointMaterial);
            waypoint.LineRenderer.material = new Material(lineMaterial);
        }

        private static void BuildLines(PathController controller)
        {
            switch (controller.typeOfPath)
            {
                case EPathsType.Waypoint:
                    BuildWaypointsLines(controller.Waypoints.Values);
                    break;
                case EPathsType.Trigger:
                    BuildTriggerLines(controller.Waypoints.Values);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void BuildWaypointsLines(IEnumerable<WaypointParts> waypointParts)
        {
            List<Vector3> positions = waypointParts.Select(wp => wp.MeshRenderer.transform.position).ToList();
            for (int i = 0; i < waypointParts.Count() - 1; i++)
            {
                WaypointParts parts = waypointParts.ElementAt(i);
                LineRenderer lr = parts.LineRenderer;
                lr.enabled = true;
                lr.SetPositions(new[] {positions[i], positions[i + 1]});
            }
        }

        private static void BuildTriggerLines(IEnumerable<WaypointParts> waypointParts)
        {
            List<Vector3> positions = waypointParts.Select(wp => wp.MeshRenderer.transform.position).ToList();
            for (int i = 1; i < waypointParts.Count(); i++)
            {
                WaypointParts parts = waypointParts.ElementAt(i);
                LineRenderer lr = parts.LineRenderer;
                lr.enabled = true;
                lr.SetPositions(new[] {positions[0], positions[i]});
            }
        }

        private static GameObject DrawWaypoint(Vector3 waypointPosition, PathController controller)
        {
            GameObject newWaypoint = new($"Waypoint_{waypointPosition.x}_{waypointPosition.y}_{waypointPosition.z}")
            {
                transform =
                {
                    localScale = _waypointScale,
                    position = waypointPosition
                }
            };

            MeshFilter mf = newWaypoint.AddComponent<MeshFilter>();
            mf.mesh = _waypointMesh;
            MeshRenderer mr = newWaypoint.AddComponent<MeshRenderer>();
            mr.material = new Material(_normalMaterial);

            LineRenderer lr = newWaypoint.AddComponent<LineRenderer>();
            lr.enabled = false;
            lr.material = new Material(_lineNormalMaterialMap[controller.typeOfPath]);
            lr.numCapVertices = 5;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.allowOcclusionWhenDynamic = false;
            lr.textureMode = LineTextureMode.Tile;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.001f;

            controller.Waypoints.Add(newWaypoint, new WaypointParts()
            {
                MeshRenderer = mr,
                LineRenderer = lr
            });

            return newWaypoint;
        }
    }
    
    public class WaypointParts
    {
        public LineRenderer LineRenderer;
        public MeshRenderer MeshRenderer;
    }
}