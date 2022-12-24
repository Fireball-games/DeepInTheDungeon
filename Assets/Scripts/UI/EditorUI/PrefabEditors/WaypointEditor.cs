using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Unity.VisualScripting;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class WaypointEditor : UIElementBase
    {
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject waypointPrefab;

        private Dictionary<WaypointControl, int> _map;

        private WaypointControl _startWaypoint;
        private WaypointControl _endWaypoint;
        private float _step;

        private void Awake()
        {
            _map = new Dictionary<WaypointControl, int>();

            _startWaypoint = body.transform.Find("StartWaypoint").GetComponent<WaypointControl>();
            _endWaypoint = body.transform.Find("EndWaypoint").GetComponent<WaypointControl>();
        }

        public void SetActive(bool isActive, List<Waypoint> waypoints)
        {
            base.SetActive(isActive);

            BuildWaypointsControls(waypoints);
        }

        private void BuildWaypointsControls(List<Waypoint> waypoints)
        {
            // clear _map
            SetWaypointActive(_startWaypoint, false);
            SetWaypointActive(_endWaypoint, false);
            
            _map.Clear();
            
            // New Path with only start point
            if (waypoints.Count == 1)
            {
                SetWaypointActive(_startWaypoint, true);
                _map.Add(_startWaypoint, 0);
                // TODO: Add add end point
                return;
            }
            
            // Typical path (2+ waypoints)
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
            control.Set(title, step, waypoint.position, waypoint.moveSpeedModifier, OnPositionChanged, OnSpeedChanged);
        }

        private void SetWaypointActive(WaypointControl waypoint, bool isActive)
        {
            waypoint.transform.parent = isActive ? scrollViewContent : body.transform;
            waypoint.gameObject.SetActive(isActive);
        }

        private void OnPositionChanged(WaypointControl control, Vector3 newPoint)
        {
            Logger.Log($"Position changed: {newPoint}");
        }

        private void OnSpeedChanged(WaypointControl control, float newSpeed)
        {
            Logger.Log($"Speed changed: {newSpeed}");
        }
    }
}