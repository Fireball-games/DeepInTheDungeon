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

        private float _step = 0.1f;

        private void Awake()
        {
            _map = new Dictionary<WaypointControl, int>();
        }

        public void SetActive(bool isActive, List<Waypoint> waypoints)
        {
            base.SetActive(isActive);

            BuildWaypointsControls(waypoints);
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
                t.Get(Keys.Floors),
                t.Get(Keys.Rows),
                t.Get(Keys.Columns)
                );
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