using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor.Services;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class WaypointEditor : UIElementBase
    {
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject waypointPrefab;

        private Dictionary<WaypointControl, int> _map;
        private List<Waypoint> _currentPath;

        private event Action<IEnumerable<Waypoint>> OnPathChanged; 

        private float _step = 0.1f;

        private void Awake()
        {
            _map = new Dictionary<WaypointControl, int>();
        }

        public void SetActive(bool isActive, IEnumerable<Waypoint> waypoints, Action<IEnumerable<Waypoint>> onPathChanged)
        {
            base.SetActive(isActive);

            OnPathChanged = null;
            OnPathChanged += onPathChanged;

            _currentPath = waypoints.ToList();
            BuildWaypointsControls(_currentPath);
        }

        private void BuildWaypointsControls(List<Waypoint> waypoints)
        {
            // clear _map
            scrollViewContent.gameObject.DismissAllChildrenToPool(true);
            
            _map.Clear();
            
            for (int i = 0; i < waypoints.Count; i++)
            {
                GameObject controlGo = ObjectPool.Instance.GetFromPool(waypointPrefab, scrollViewContent.gameObject, true);
                WaypointControl control = controlGo.GetComponent<WaypointControl>();

                string title = i == 0 ? t.Get(Keys.StartPoint) : i == waypoints.Count - 1 ? t.Get(Keys.EndPoint) : $"{Keys.Point} {i + 1}";
                float step = i == 0 || i == waypoints.Count - 1 ? 1f : _step;
                SetWaypoint(control, title, step, waypoints[i]);
                
                _map.Add(control, i);
            }
        }

        private void SetWaypoint(WaypointControl control, string title, float step, Waypoint waypoint)
        {
            control.Set(title,
                step,
                waypoint.position,
                waypoint.moveSpeedModifier,
                OnPositionChanged,
                OnSpeedChanged,
                t.Get(Keys.Rows),
                t.Get(Keys.Floors),
                t.Get(Keys.Columns)
                );
        }

        private void OnPositionChanged(WaypointControl control, Vector3 newPoint)
        {
            _currentPath[_map[control]].position = newPoint;
            
            OnPathChanged?.Invoke(_currentPath);
            // WayPointService.HighlightPoint(_currentPath[0].position.ToVector3Int(), _map[control], isExclusiveHighlight: true);
        }

        private void OnSpeedChanged(WaypointControl control, float newSpeed)
        {
            _currentPath[_map[control]].moveSpeedModifier = newSpeed;
            
            OnPathChanged?.Invoke(_currentPath);
            // WayPointService.HighlightPoint(_currentPath[0].position.ToVector3Int(), _map[control], isExclusiveHighlight: true);
        }
    }
}