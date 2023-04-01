using System;

namespace Scripts.InventoryManagement.Inventories
{
    /// <summary>
    /// Locations on the players body where items can be equipped.
    /// </summary>
    [Flags]
    public enum EquipLocation
    {
        Helmet = 1 << 0,
        Necklace = 1 << 1,
        Body = 1 << 2,
        Trousers = 1 << 3,
        Boots = 1 << 4,
        WeaponLeft = 1 << 5,
        WeaponRight = 1 << 6,
        Shield = 1 << 7,
        Gloves = 1 << 8,
        Ring1 = 1 << 9,
        Ring2 = 1 << 10,
        Throwable1 = 1 << 11,
        Throwable2 = 1 << 12,
        Throwable3 = 1 << 13,
    }
}