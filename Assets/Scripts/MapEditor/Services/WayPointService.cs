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
        [SerializeField] private Material waypointLinesMaterial;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private Vector3 waypointScale = new(0.3f, 0.3f, 0.3f);

        private static Dictionary<Vector3Int, PathController> _paths;
        private static Mesh _waypointMesh;
        private static Vector3 _waypointScale;
        private static Material _normalMaterial;
        private static Material _highlightedMaterial;
        private static Material _waypointsLineMaterial;
        private static GameObject _parent;
        private static bool _areWaypointsShows;

        private void Awake()
        {
            _paths = new Dictionary<Vector3Int, PathController>();
            _parent = new GameObject("Waypoints");
            _waypointMesh = waypointPrefab.GetComponent<MeshFilter>().sharedMesh;
            _waypointScale = waypointScale;
            _normalMaterial = normalMaterial;
            _highlightedMaterial = highlightedMaterial;
            _waypointsLineMaterial = waypointLinesMaterial;
        }

        public static void ShowWaypoints()
        {
            _areWaypointsShows = true;
            _parent.SetActive(true);
        }

        public static void HideWaypoints()
        {
            _areWaypointsShows = false;
            _parent.SetActive(false);
        }

        public static void HighlightPath(Vector3Int startPoint, bool isHighlighted = true)
        {
            if (!_paths.TryGetValue(startPoint, out PathController controller) || controller.IsHighlighted == isHighlighted) return;

            controller.IsHighlighted = isHighlighted;
            
            for (int index = 0; index < controller.Waypoints.Count; index++)
            {
                HighlightPoint(startPoint, index, isHighlighted);
            }
        }
        
        public static void HighlightPoint(Vector3Int startPoint, int pointIndex, bool isHighlighted = true)
        {
            HighlightPoint(_paths[startPoint].Waypoints.ElementAt(pointIndex).Value, isHighlighted);
        }
        
        public static void DestroyPath(Vector3Int startPoint)
        {
            if (!_paths.ContainsKey(startPoint)) return;
            
            Destroy(_paths[startPoint].gameObject);
            _paths.Remove(startPoint);
        }
        
        public static void AddPath(IEnumerable<Waypoint> waypoints, bool highlightAfterBuild = false)
        {
            Vector3 startPoint = waypoints.ElementAt(0).position;
            
            DestroyPath(startPoint.ToVector3Int());

            GameObject pathParent = new($"Path_{waypoints.Count()}x_{startPoint.x}_{startPoint.y}_{startPoint.z}")
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
            
            _paths.Add(startPoint.ToVector3Int(), controller);

            BuildLines(controller);

            if (highlightAfterBuild)
            {
                HighlightPath(controller);
            }
        }

        private static void HighlightPath(PathController pathController)
        {
            HighlightPath(_paths.GetFirstKeyByValue(pathController));
        }

        private static void HighlightPoint(WaypointParts waypoint, bool isHighlighted = true)
        {
            if (isHighlighted)
            {
                waypoint.MeshRenderer.material = _highlightedMaterial;
                return;
            }

            waypoint.MeshRenderer.material = _normalMaterial;
            // TODO: add line renderer highlight
        }

        private static void BuildLines(PathController controller)
        {
            List<Vector3> positions = controller.Waypoints.Values.Select(wp => wp.MeshRenderer.transform.position).ToList();
            for (int i = 1; i < controller.Waypoints.Count - 1; i++)
            {
                WaypointParts parts = controller.Waypoints.ElementAt(i).Value;
                LineRenderer lr = parts.LineRenderer;
                lr.enabled = true;
                lr.SetPositions(new []{positions[i], positions[i + 1]});
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
            mr.material = _normalMaterial;

            LineRenderer lr = newWaypoint.AddComponent<LineRenderer>();
            lr.enabled = false;
            lr.material = _waypointsLineMaterial;
            lr.numCapVertices = 5;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.allowOcclusionWhenDynamic = false;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.001f;

            controller.Waypoints.Add(newWaypoint, new WaypointParts()
            {
                MeshRenderer = mr,
                LineRenderer = lr
            });

            return newWaypoint;
        }
    }
}