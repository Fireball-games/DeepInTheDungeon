using Scripts.EventsManagement;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class WallGizmoController : MonoBehaviour
    {
        [SerializeField] private WallGizmo northGizmo;
        [SerializeField] private WallGizmo eastGizmo;
        [SerializeField] private WallGizmo southGizmo;
        [SerializeField] private WallGizmo westGizmo;
        [SerializeField] private GameObject wall;

        private EditorMouseService Mouse => EditorMouseService.Instance;

        private void OnEnable()
        {
            EditorEvents.OnMouseGridPositionChanged += OnMouseGridPositionChanged;
        }

        private void OnDisable()
        {
            EditorEvents.OnMouseGridPositionChanged -= OnMouseGridPositionChanged;
        }

        private void OnMouseGridPositionChanged(Vector3Int current, Vector3Int previous)
        {
            if (Mouse.GridPositionType is Enums.EGridPositionType.EditableTile &&
                MapEditorManager.Instance.WorkMode is Enums.EWorkMode.Walls)
            {
                
            }
        }
    }
}
