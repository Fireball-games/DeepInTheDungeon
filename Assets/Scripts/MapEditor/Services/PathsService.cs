using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;
using PathsType = System.Collections.Generic.Dictionary<Scripts.MapEditor.Services.PathsService.EPathsType, System.Collections.Generic.Dictionary<(UnityEngine.Vector3, UnityEngine.Vector3), Scripts.MapEditor.PathController>>;

namespace Scripts.MapEditor.Services
{
    public class PathsService : MonoBehaviour
    {
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightedMaterial;
        [SerializeField] private Material waypointLinesNormalMaterial;
        [SerializeField] private Material waypointLinesHighlightedMaterial;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private Vector3 waypointScale = new(0.3f, 0.3f, 0.3f);

        private static PathsType _paths;
        private static Mesh _waypointMesh;
        private static Vector3 _waypointScale;
        private static Material _normalMaterial;
        private static Material _highlightedMaterial;
        private static Material _waypointsLineNormalMaterial;
        private static Material _waypointLinesHighlightedMaterial;
        private static GameObject _parent;
        private static bool _areWaypointsShows = true;

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
            
            _parent = new GameObject("Waypoints");
            _waypointMesh = waypointPrefab.GetComponent<MeshFilter>().sharedMesh;
            _waypointScale = waypointScale;
            _normalMaterial = normalMaterial;
            _highlightedMaterial = highlightedMaterial;
            _waypointsLineNormalMaterial = waypointLinesNormalMaterial;
            _waypointLinesHighlightedMaterial = waypointLinesHighlightedMaterial;
        }

        public static void ShowPaths(EPathsType pathsType)
        {
            _areWaypointsShows = true;
            foreach (PathController controller in _paths[pathsType].Values)
            {
                controller.gameObject.SetActive(true);
            }
        }

        public static void HidePaths(EPathsType pathsType)
        {
            _areWaypointsShows = false;
            
            foreach (PathController controller in _paths[pathsType].Values.Where(controller => !controller.IsHighlighted))
            {
                controller.gameObject.SetActive(false);
            }
        }

        public static void HighlightPath(EPathsType pathsType, List<Waypoint> path, bool isHighlighted = true) =>
            HighlightPath(pathsType, GetKey(path), isHighlighted);

        public static void HighlightPoint(EPathsType pathType, List<Waypoint> path, int pointIndex, bool isHighlighted = true, bool isExclusiveHighlight = false) =>
            HighlightPoint(pathType, GetKey(path), pointIndex, isHighlighted, isExclusiveHighlight);

        public static void DestroyPath(EPathsType pathType, List<Waypoint> path) => DestroyPath(pathType, GetKey(path));

        public static void AddWaypointPath(EPathsType pathType, List<Waypoint> waypoints, bool highlightAfterBuild = false)
        {
            (Vector3, Vector3) key = GetKey(waypoints);
            DestroyPath(pathType, key);

            GameObject pathParent = new($"Path_{key.Item1}_{key.Item2}")
            {
                transform =
                {
                    parent = _parent.transform
                }
            };

            PathController controller = pathParent.AddComponent<PathController>();

            foreach (Waypoint waypoint in waypoints)
            {
                GameObject drawnWaypoint = DrawWaypoint(waypoint.position, controller);
                drawnWaypoint.transform.parent = pathParent.transform;
            }

            _paths[EPathsType.Waypoint].Add(GetKey(waypoints), controller);

            BuildLines(controller);

            if (highlightAfterBuild)
            {
                HighlightPath(EPathsType.Waypoint, controller);
                return;
            }
            
            if (!_areWaypointsShows)
                HidePaths(EPathsType.Waypoint);
        }
        
        public static void DestroyAllPaths()
        {
            foreach (EPathsType pathType in new PathsType(_paths).Keys)
            {
                foreach ((Vector3, Vector3) key in new Dictionary<(Vector3,Vector3), PathController>(_paths[pathType]).Keys)
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
        
        private static void HighlightPath(EPathsType pathType, (Vector3, Vector3) key, bool isHighlighted = true)
        {
            if (!_paths[pathType].TryGetValue(key, out PathController controller) || controller.IsHighlighted == isHighlighted) return;

            controller.IsHighlighted = isHighlighted;

            for (int index = 0; index < controller.Waypoints.Count; index++)
            {
                HighlightPoint(pathType, key, index, isHighlighted);
            }
        }
        
        private static void HighlightPoint(EPathsType pathType, (Vector3, Vector3) key, int pointIndex, bool isHighlighted = true, bool isExclusiveHighlight = false)
        {
            Dictionary<GameObject, WaypointParts> waypointsParts = _paths[pathType][key].Waypoints;
            
            if (isExclusiveHighlight)
            {
                for (int index = 0; index < waypointsParts.Count; index++)
                {
                    HighlightPoint(waypointsParts.ElementAt(pointIndex).Value, index == pointIndex);
                }
            }
            
            HighlightPoint(waypointsParts.ElementAt(pointIndex).Value, isHighlighted);
        }
        
        private static void DestroyPath(EPathsType pathType, (Vector3, Vector3) key)
        {
            if (!_paths[pathType].ContainsKey(key)) return;

            Destroy(_paths[pathType][key].gameObject);
            _paths[pathType].Remove(key);
        }

        private static (Vector3, Vector3) GetKey(List<Waypoint> waypoints)
        {
            return waypoints.Count == 1 
                ? (waypoints[0].position.ToVector3Int(), Vector3.negativeInfinity) 
                : new ValueTuple<Vector3, Vector3>(waypoints[0].position.Round(2), waypoints[1].position.Round(2));
        }

        private static void HighlightPath(EPathsType pathType, PathController pathController, bool isHighlighted = true)
        {
            HighlightPath(pathType, _paths[pathType].GetFirstKeyByValue(pathController), isHighlighted);
        }

        private static void HighlightPoint(WaypointParts waypoint, bool isHighlighted = true)
        {
            Material pointMaterial = isHighlighted ? _highlightedMaterial : _normalMaterial;
            Material lineMaterial = isHighlighted ? _waypointLinesHighlightedMaterial : _waypointsLineNormalMaterial;

            waypoint.MeshRenderer.material = new Material(pointMaterial);
            waypoint.LineRenderer.material = new Material(lineMaterial);
        }

        private static void BuildLines(PathController controller)
        {
            List<Vector3> positions = controller.Waypoints.Values.Select(wp => wp.MeshRenderer.transform.position).ToList();
            for (int i = 0; i < controller.Waypoints.Count - 1; i++)
            {
                WaypointParts parts = controller.Waypoints.ElementAt(i).Value;
                LineRenderer lr = parts.LineRenderer;
                lr.enabled = true;
                lr.SetPositions(new[] {positions[i], positions[i + 1]});
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
            lr.material = new Material(_waypointsLineNormalMaterial);
            lr.numCapVertices = 5;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.allowOcclusionWhenDynamic = false;
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