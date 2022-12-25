using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.Components;
using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class WaypointEditor : UIElementBase
    {
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private GameObject addWaypointPrefab;

        private Dictionary<WaypointControl, int> _map;
        private List<Waypoint> _currentPath;

        private event Action<IEnumerable<Waypoint>> OnPathChanged;

        private float _step = 0.1f;

        public enum EAddWaypointType
        {
            BeforeLast,
            EndPoint,
        }

        private void Awake()
        {
            _map = new Dictionary<WaypointControl, int>();
        }

        public void SetActive(bool isActive, IEnumerable<Waypoint> waypoints, Action<IEnumerable<Waypoint>> onPathChanged)
        {
            base.SetActive(isActive);

            OnPathChanged = null;
            OnPathChanged += onPathChanged;

            _currentPath = Waypoint.Clone(waypoints).ToList();
            BuildWaypointsControls(_currentPath);
        }

        private void BuildWaypointsControls(List<Waypoint> waypoints)
        {
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
                
                if (i == waypoints.Count - 2)
                {
                    AddAddWaypointWidget(t.Get(Keys.AddWaypoint), EAddWaypointType.BeforeLast);
                }
            }
            
            if (!IsEndPointValid())
            {
                AddAddWaypointWidget(t.Get(Keys.AddEndPoint), EAddWaypointType.EndPoint);
            }
        }

        private void AddAddWaypointWidget(string labelText, EAddWaypointType type)
        {
            AddWaypointWidget widget = ObjectPool.Instance
                .GetFromPool(addWaypointPrefab, scrollViewContent.gameObject, true)
                .GetComponent<AddWaypointWidget>();
            widget.Set(labelText, type, OnAddWaypointClicked);
        }

        private bool IsEndPointValid()
        {
            Vector3 endPoint = _currentPath[^1].position;
            return Mathf.Abs(endPoint.x % 1) < float.Epsilon
                   && Mathf.Abs(endPoint.y % 1) < float.Epsilon
                   && Mathf.Abs(endPoint.z % 1) < float.Epsilon;
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
        }

        private void OnSpeedChanged(WaypointControl control, float newSpeed)
        {
            _currentPath[_map[control]].moveSpeedModifier = newSpeed;

            OnPathChanged?.Invoke(Waypoint.Clone(_currentPath));
        }

        private void OnAddWaypointClicked(Vector3 position, EAddWaypointType type)
        {
            Waypoint newWaypoint = new(position + _currentPath[^2].position, 0.3f);
            
            if (type is EAddWaypointType.BeforeLast)
            {
                _currentPath[^1].position += (_currentPath[^1].position - _currentPath[^2].position).normalized;
                _currentPath.Insert(_currentPath.Count - 1, newWaypoint);
            }
            else
            {
                _currentPath.Add(newWaypoint);
            }
            
            BuildWaypointsControls(_currentPath);
            OnPathChanged?.Invoke(Waypoint.Clone(_currentPath));
        }
    }
}