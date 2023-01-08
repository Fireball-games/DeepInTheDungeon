namespace Scripts.UI.EditorUI.PrefabEditors
{
    public interface IPrefabEditor
    {
        public void Open();
        public void CloseWithRemovingChanges();
    }
}