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
using UnityEngine.Events;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class WaypointEditor : UIElementBase
    {
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private GameObject addWaypointPrefab;

        private StepSelector _stepSelector;
        
        private Dictionary<WaypointControl, int> _map;
        private List<Waypoint> _currentPath;

        private event Action<IEnumerable<Waypoint>, int> OnPathChanged;

        private float _step = 0.5f;

        public enum EAddWaypointType
        {
            BeforeLast,
            EndPoint,
        }

        private void Awake()
        {
            _stepSelector = body.transform.Find("Background/Frame/StepSelector").GetComponent<StepSelector>();
            _stepSelector.Set(StepSelector.EStep.Step05, OnStepChanged);
            
            body.transform.Find("Background/Frame/Title").GetComponent<Title>().SetTitle(t.Get(Keys.WaypointEditor));
            
            _map = new Dictionary<WaypointControl, int>();
        }

        public void SetActive(bool isActive, IEnumerable<Waypoint> waypoints, Action<IEnumerable<Waypoint>, int> onPathChanged)
        {
            base.SetActive(isActive);

            OnPathChanged = null;
            OnPathChanged += onPathChanged;

            _currentPath = Waypoint.CloneCollection(waypoints).ToList();
            BuildWaypointsControls(_currentPath);
        }

        private void BuildWaypointsControls(List<Waypoint> waypoints)
        {
            scrollViewContent.gameObject.DismissAllChildrenToPool();

            _map.Clear();

            for (int i = 0; i < waypoints.Count; i++)
            {
                GameObject controlGo = ObjectPool.Instance.Get(waypointPrefab, scrollViewContent.gameObject);
                WaypointControl control = controlGo.GetComponent<WaypointControl>();

                string title = i == 0 ? t.Get(Keys.StartPoint) : i == waypoints.Count - 1 ? t.Get(Keys.EndPoint) : $"{Keys.Point} {i + 1}";
                float step = i == 0 || i == waypoints.Count - 1 ? 1f : _step;
                bool isDeleteButtonActive = i != 0 && i != _currentPath.Count - 1;
                UnityAction<WaypointControl> onDeleteButtonClicked = isDeleteButtonActive ? OnDeleteButtonClicked : null;

                if (i == 0 || i == _currentPath.Count - 1)
                {
                    waypoints[i].position = waypoints[i].position.ToVector3Int();
                }

                SetWaypoint(control, title, step, waypoints[i], isDeleteButtonActive, onDeleteButtonClicked);

                _map.Add(control, i);

                if (waypoints.Count == 1)
                {
                    AddAddWaypointWidget(t.Get(Keys.AddEndPoint), EAddWaypointType.EndPoint);
                }
                else if (i == waypoints.Count - 2)
                {
                    AddAddWaypointWidget(t.Get(Keys.AddWaypoint), EAddWaypointType.BeforeLast);
                }
            }
        }

        private void AddAddWaypointWidget(string labelText, EAddWaypointType type)
        {
            AddWaypointWidget widget = ObjectPool.Instance
                .Get(addWaypointPrefab, scrollViewContent.gameObject)
                .GetComponent<AddWaypointWidget>();

            widget.Set(labelText, type, OnAddWaypointClicked);
        }

        private void SetWaypoint(WaypointControl control,
            string title,
            float step,
            Waypoint waypoint,
            bool isDeleteButtonActive,
            UnityAction<WaypointControl> onDeleteButtonClicked)
        {
            control.Set(title,
                step,
                waypoint.position,
                waypoint.moveSpeedModifier,
                OnPositionChanged,
                OnSpeedChanged,
                isDeleteButtonActive,
                onDeleteButtonClicked
            );
        }

        private void OnPositionChanged(WaypointControl control, Vector3 newPoint)
        {
            int lastEditedPointIndex = _map[control];
            _currentPath[lastEditedPointIndex].position = newPoint;

            OnPathChanged?.Invoke(Waypoint.CloneCollection(_currentPath), lastEditedPointIndex);
        }

        private void OnSpeedChanged(WaypointControl control, float newSpeed)
        {
            int lastEditedPointIndex = _map[control];
            _currentPath[lastEditedPointIndex].moveSpeedModifier = newSpeed;

            OnPathChanged?.Invoke(Waypoint.CloneCollection(_currentPath), lastEditedPointIndex);
        }

        private void OnDeleteButtonClicked(WaypointControl control)
        {
            _currentPath.RemoveAt(_map[control]);
            BuildWaypointsControls(_currentPath);
            OnPathChanged?.Invoke(Waypoint.CloneCollection(_currentPath), -1);
        }

        private void OnAddWaypointClicked(Vector3 direction, EAddWaypointType type)
        {
            Vector3 offsetPosition = _currentPath.Count == 1 ? _currentPath[0].position : _currentPath[^2].position;
            float step = _currentPath.Count == 1 ? 1 : _step;
            Vector3 newPointPosition = (direction * step + offsetPosition).Round(2);
            Waypoint newWaypoint = new(newPointPosition, 0.3f);

            if (type is EAddWaypointType.BeforeLast)
            {
                if (_currentPath.Count == 1)
                {
                    _currentPath.Add(newWaypoint);
                }
                else
                {
                    if (newPointPosition == _currentPath[^1].position)
                    {
                        _currentPath[^1].position += (_currentPath[^1].position - _currentPath[^2].position).normalized;
                    }

                    _currentPath.Insert(_currentPath.Count - 1, newWaypoint);
                }
            }
            else
            {
                _currentPath.Add(newWaypoint);
            }

            BuildWaypointsControls(_currentPath);
            OnPathChanged?.Invoke(Waypoint.CloneCollection(_currentPath), type is EAddWaypointType.EndPoint ? _currentPath.Count - 1 : _currentPath.Count - 2);
        }

        private void OnStepChanged(float newStep)
        {
            WaypointControl[] children = scrollViewContent.GetComponentsInChildren<WaypointControl>();
            _step = newStep;
            
            for (int index = 0; index < children.Length; index++)
            {
                if (index == 0 || index == children.Length - 1) continue;
                
                WaypointControl control = children[index];
                control.Step = newStep;
            }
        }
    }
}