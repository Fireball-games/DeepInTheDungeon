using UnityEngine;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class PrefabPreview : PreviewWindowBase
    {
        public void Show(GameObject prefab, Preview3D.EPreviewType previewType) => Show(prefab, prefab.name, previewType);
    }
}