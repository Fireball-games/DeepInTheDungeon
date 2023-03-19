using System.Collections.Generic;
using Scripts.Helpers;
using Scripts.Inventory.Inventories.Items;

namespace Scripts.UI.EditorUI.PrefabEditors
{
    public class ItemEditor : MapPartsEditorWindowBase
    {
        
        public override void Open()
        {
            IEnumerable<MapObject> items = MapObject.GetAllItems();
        }

        protected override void RemoveAndClose()
        {
            Logger.LogNotImplemented();
        }
    }
}