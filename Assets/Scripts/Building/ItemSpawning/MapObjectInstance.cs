using Scripts.InventoryManagement.Inventories.Items;
using Scripts.System;
using Scripts.System.MonoBases;

namespace Scripts.Building.ItemSpawning
{
    public class MapObjectInstance : MonoBase
    {
        public MapObject Item { get; private set; }
        public string ItemID => Item.GetItemID();

        protected void Setup(MapObject item)
        {
            Item = item;
        }
        
        public void SetTransform(PositionRotation positionRotation)
        {
            transform.position = positionRotation.Position;
            transform.localRotation = positionRotation.Rotation;
        }
    }
}