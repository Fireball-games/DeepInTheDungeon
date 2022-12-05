using System.Collections;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.MapEditor
{
    public class MouseService : MonoBehaviour
    {
        public static MouseService instance;
        public Vector3Int MouseGridPosition => _lastGridPosition;
        
        private Plane _layerPlane;
        private Vector3Int _lastGridPosition;
        private WaitForSecondsRealtime _refreshPeriod;

        protected void Awake()
        {
            _layerPlane = new Plane(Vector3.up, Vector3.zero);
            _lastGridPosition = new Vector3Int(-1000, -1000, -1000);
            _refreshPeriod = new WaitForSecondsRealtime(0.1f);
        }

        private void Start()
        {
            StartCoroutine(UpdateMousePositionCoroutine());
        }
        
        public static MouseService Instance
        {
            get
            {
                if (instance) return instance;
                
                instance = FindObjectOfType<MouseService> ();
                    
                if (instance) return instance;
                    
                GameObject obj = new()
                {
                    name = nameof(MouseService)
                };
                
                instance = obj.AddComponent<MouseService> ();
                return instance;
            }
        }

        private IEnumerator UpdateMousePositionCoroutine()
        {
            while (true)
            {
                Vector3Int newGridPosition = Extensions.Vector3IntZero;
                
                Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
                if (_layerPlane.Raycast(ray, out float distance))
                {
                    newGridPosition = ray.GetPoint(distance).ToVector3Int();
                }

                if (!newGridPosition.Equals(_lastGridPosition))
                {
                    _lastGridPosition = newGridPosition;
                    // Logger.Log($"NewMouseGridPosition: {_lastGridPosition}");
                    EditorEvents.TriggerOnMouseGridPositionChanged(newGridPosition, _lastGridPosition);
                }
                
                yield return _refreshPeriod;
            }
        }
    }
}
