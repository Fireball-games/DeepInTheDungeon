using Scripts.Helpers.Extensions;
using UnityEngine;

namespace Scripts.MapEditor
{
    public class Cursor3D : CageController
    {
        [SerializeField] private GameObject copy;

        public static Vector3 EditorWallCursorScale;

        static Cursor3D()
        {
            EditorWallCursorScale = new Vector3(0.15f, 1.2f, 1.2f);
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

        public override void Hide()
        {
            base.Hide();
            
            copy.SetActive(false);
        }
    }
}