using Scripts.Building.PrefabsSpawning;
using Scripts.UI.EditorUI.PrefabEditors;
using UnityEngine;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabList : ListWindowBase<PrefabBase, PrefabListButton>
    {
        private PrefabPreview _preview;
        
        protected override string GetItemIdentification(PrefabBase item) => item.gameObject.name;

        protected override void SetButton(PrefabListButton button, PrefabBase item)
        {
            base.SetButton(button, item);
            
            button.SetMousePointerMethods(OnButtonMouseEnter, OnButtonMouseExit);
        }

        private void OnButtonMouseEnter(GameObject prefabGo)
        {
            _preview ??= GetComponentInChildren<PrefabPreview>();
            
            if (!_preview) return;
            
            _preview.Show(prefabGo, Preview3D.EPreviewType.Tile);
        }

        private void OnButtonMouseExit()
        {
            if (!_preview) return;
            
            _preview.Hide();
        }
    }
}