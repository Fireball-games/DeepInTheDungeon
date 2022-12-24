using System;
using System.Collections.Generic;
using Scripts.Helpers.Extensions;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class WaypointEditor : UIElementBase
    {
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject waypointPrefab;

        private Dictionary<int, WaypointControl> _map;

        private WaypointControl _startWaypoint;
        private WaypointControl _endWaypoint;
        private float _step;

        private void Awake()
        {
            _map = new Dictionary<int, WaypointControl>();

            _startWaypoint = body.transform.Find("StartWaypoint").GetComponent<WaypointControl>();
            _startWaypoint.Set("placeholder"
                , _step,
                v3 => Logger.Log("position Changed"),
                s => Logger.Log("speed changed"), "Px", "Py", "Pz");
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
            if (_map.Count > 1)
            {
                for (int i = 0; i < _map.Count; i++)
                {
                    WaypointControl currentControl = _map[i];

                    if (i == 0 || i == _map.Count - 1)
                    {
                        SetWaypointActive(currentControl, false);
                        continue;
                    }
                    
                    ObjectPool.Instance.ReturnToPool(currentControl.gameObject, true);
                }
            }
            else
            {
                SetWaypointActive(_startWaypoint, false);
            }
            
            _map.Clear();
            
            // New Path with only start point
            if (waypoints.Count == 1)
            {
                SetWaypointActive(_startWaypoint, true);
                return;
            }
            
            // Typical path (2+ waypoints)
            for (int i = 0; i < waypoints.Count; i++)
            {
                WaypointControl currentControl;
                if (i == 0)
                {
                    SetWaypointActive(_startWaypoint, true);
                    // TODO: setting step, speed and value directly in WaypointControl
                    // _startWaypoint;
                }
                
                
            }
        }

        private void SetWaypointActive(WaypointControl waypoint, bool isActive)
        {
            waypoint.transform.parent = isActive ? scrollViewContent : body.transform;
            waypoint.gameObject.SetActive(isActive);
        }
    }
}