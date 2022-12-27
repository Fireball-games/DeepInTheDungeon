using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scripts.MapEditor.Services
{
    public class WayPointService : MonoBehaviour
    {
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightedMaterial;
        [SerializeField] private Material waypointLinesNormalMaterial;
        [SerializeField] private Material waypointLinesHighlightedMaterial;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private Vector3 waypointScale = new(0.3f, 0.3f, 0.3f);

        private static Dictionary<(Vector3, Vector3), PathController> _paths;
        private static Mesh _waypointMesh;
        private static Vector3 _waypointScale;
        private static Material _normalMaterial;
        private static Material _highlightedMaterial;
        private static Material _waypointsLineNormalMaterial;
        private static Material _waypointLinesHighlightedMaterial;
        private static Color _pathColorStart;
        private static Color _pathColorEnd;
        private static GameObject _parent;
        private static bool _areWaypointsShows = true;

        private void Awake()
        {
            _paths = new Dictionary<(Vector3, Vector3), PathController>();
            _parent = new GameObject("Waypoints");
            _waypointMesh = waypointPrefab.GetComponent<MeshFilter>().sharedMesh;
            _waypointScale = waypointScale;
            _normalMaterial = normalMaterial;
            _highlightedMaterial = highlightedMaterial;
            _waypointsLineNormalMaterial = waypointLinesNormalMaterial;
            _waypointLinesHighlightedMaterial = waypointLinesHighlightedMaterial;
        }

        public static void ShowWaypoints()
        {
            _areWaypointsShows = true;
            foreach (PathController controller in _paths.Values)
            {
                controller.gameObject.SetActive(true);
            }
        }

        public static void HideWaypoints()
        {
            _areWaypointsShows = false;
            
            foreach (PathController controller in _paths.Values.Where(controller => !controller.IsHighlighted))
            {
                controller.gameObject.SetActive(false);
            }
        }

        private static void HighlightPath((Vector3, Vector3) key, bool isHighlighted = true)
        {
            if (!_paths.TryGetValue(key, out PathController controller) || controller.IsHighlighted == isHighlighted) return;

            controller.IsHighlighted = isHighlighted;

            for (int index = 0; index < controller.Waypoints.Count; index++)
            {
                HighlightPoint(key, index, isHighlighted);
            }
        }

        public static void HighlightPath(List<Waypoint> path, bool isHighlighted = true) =>
            HighlightPath(GetKey(path), isHighlighted);

        public static void HighlightPoint((Vector3, Vector3) key, int pointIndex, bool isHighlighted = true, bool isExclusiveHighlight = false)
        {
            if (isExclusiveHighlight)
            {
                for (int index = 0; index < _paths[key].Waypoints.Count; index++)
                {
                    HighlightPoint(_paths[key].Waypoints.ElementAt(pointIndex).Value, index == pointIndex);
                }
            }
            
            HighlightPoint(_paths[key].Waypoints.ElementAt(pointIndex).Value, isHighlighted);
        }

        public static void DestroyPath(List<Waypoint> path) => DestroyPath(GetKey(path));

        public static void DestroyPath((Vector3, Vector3) key)
        {
            if (!_paths.ContainsKey(key)) return;

            Destroy(_paths[key].gameObject);
            _paths.Remove(key);
        }

        public static void AddPath(List<Waypoint> waypoints, bool highlightAfterBuild = false)
        {
            (Vector3, Vector3) key = GetKey(waypoints);
            DestroyPath(key);

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

            _paths.Add(GetKey(waypoints), controller);

            BuildLines(controller);

            if (highlightAfterBuild)
            {
                HighlightPath(controller);
                return;
            }
            
            if (!_areWaypointsShows)
                HideWaypoints();
        }
        
        public static void DestroyAllPaths()
        {
            foreach ((Vector3, Vector3) key in new Dictionary<(Vector3,Vector3), PathController>(_paths).Keys)
            {
                DestroyPath(key);
            }
        }

        private static (Vector3, Vector3) GetKey(List<Waypoint> waypoints)
        {
            return waypoints.Count == 1 
                ? (waypoints[0].position.ToVector3Int(), Vector3.negativeInfinity) 
                : new ValueTuple<Vector3Int, Vector3>(waypoints[0].position.ToVector3Int(), waypoints[1].position.ToVector3Int());
        }

        private static void HighlightPath(PathController pathController, bool isHighlighted = true)
        {
            HighlightPath(_paths.GetFirstKeyByValue(pathController), isHighlighted);
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