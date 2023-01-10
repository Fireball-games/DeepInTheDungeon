using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public interface IPrefabEditor
    {
        public void Open();
        public void CloseWithRemovingChanges();
        public void MoveCameraToPrefab(Vector3 worldPosition);
    }
}