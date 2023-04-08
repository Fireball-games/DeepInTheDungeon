using System;

namespace Scripts.InventoryManagement.Inventories
{
    [Serializable]
    public struct InventorySlotRecord
    {
        public string itemID;
        public int number;

        public InventorySlotRecord(string itemID, int number)
        {
            this.itemID = itemID;
            this.number = number;
        }
    }
}