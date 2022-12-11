using Scripts.Helpers.Extensions;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class Cursor3D : MonoBehaviour
    {
        [SerializeField] private GameObject cursor;
        [SerializeField] private GameObject copy;

        private MapBuildService _service;

        private void OnEnable()
        {
            cursor.gameObject.SetActive(false);
        }

        public void SetMapBuildService(MapBuildService service)
        {
            _service = service;
        }

        public void ShowAt(Vector3Int gridPosition, bool withCopyAbove = false, bool withCopyBellow = false)
        {
            Vector3 worldPosition = gridPosition.ToWorldPosition();
            ShowAt(worldPosition);

            if (withCopyAbove)
            {
                copy.transform.position = worldPosition + Vector3.up;
                copy.SetActive(true);
                return;
            }
            
            if (withCopyBellow)
            {
                copy.transform.position = worldPosition + Vector3.down;
                copy.SetActive(true);
                return;
            }
            
            copy.SetActive(false);
        }
        
        public void ShowAt(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            // Logger.Log($"Activating cursor on worldPosition: {worldPosition}");
            cursor.SetActive(true);
        }

        public void Hide()
        {
            _service.ResetShownNullTilesColors();
            copy.SetActive(false);
            cursor.SetActive(false);
        }
    }
}