﻿using Scripts.Inventory.Inventories.Items;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEditor;

namespace Scripts.Building.ItemSpawning
{
    public class MapObjectInstance : MonoBase
    {
        public MapObject Item { get; private set; }
        public string ItemID => Item.GetItemID();
        
        public void Setup(MapObject item)
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