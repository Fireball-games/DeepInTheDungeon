using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor.Services
{
    public class WayPointService : MonoBehaviour
    {
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightedMaterial;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private Vector3 waypointScale = new(0.3f, 0.3f, 0.3f);

        private static Dictionary<Vector3Int, PathController> _paths;
        private static Mesh _waypointMesh;
        private static Vector3 _waypointScale;
        private static Material _normalMaterial;
        private static Material _highlightedMaterial;
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

        public static void HighlightPath(PathController pathController)
        {
            Logger.LogNotImplemented();
        }

        public static void AddPath(Vector3Int origin, IEnumerable<Waypoint> waypoints, bool highlightAfterBuild = false)
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
                GameObject drawnWaypoint = DrawWaypoint(waypoint.position);
                drawnWaypoint.transform.parent = pathParent.transform;
                controller.waypoints.Add(drawnWaypoint);
            }
            
            _paths.Add(origin, controller);

            if (highlightAfterBuild)
            {
                HighlightPath(controller);
            }
        }
        
        public static void DestroyPath(Vector3Int startPoint)
        {
            if (!_paths.ContainsKey(startPoint)) return;
            
            Destroy(_paths[startPoint].gameObject);
            _paths.Remove(startPoint);
        }

        private static GameObject DrawWaypoint(Vector3 waypointPosition)
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

            return newWaypoint;
        }
    }
}