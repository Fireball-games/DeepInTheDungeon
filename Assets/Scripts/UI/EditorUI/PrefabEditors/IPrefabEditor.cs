using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public interface IPrefabEditor
    {
        public void Open();
        public void MoveCameraToPrefab(Vector3 worldPosition);
        public Vector3 GetCursor3DScale();
    }
}