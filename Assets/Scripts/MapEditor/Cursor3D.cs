using Scripts.Helpers;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class Cursor3D : MonoBehaviour
    {
        [SerializeField] private GameObject cursor;

        private MapBuildService _service;

        private void OnEnable()
        {
            cursor.gameObject.SetActive(false);
        }

        public void SetMapBuildService(MapBuildService service)
        {
            _service = service;
        }

        public void ShowAt(Vector3Int griPosition)
        {
            ShowAt(griPosition.ToWorldPosition() + Vector3.up);    
        }
        
        public void ShowAt(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            cursor.SetActive(true);
        }

        public void Hide()
        {
            _service.ResetShownNullTilesColors();
            cursor.SetActive(false);
        }
    }
}