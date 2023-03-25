using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Inventory.Inventories;
using Scripts.Inventory.Inventories.Items;
using Scripts.System;
using UnityEngine;
using static Scripts.Building.ItemSpawning.MapObjectConfiguration;

namespace Scripts.Building.ItemSpawning
{
    public class ItemSpawner
    {
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        public static GameObject Parent => MapBuilder.ItemsParent;
        
        public void SpawnItem(MapObjectConfiguration item)
        {
            MapObject mapObject = MapObject.GetFromID<MapObject>(item.ID);

            if (mapObject is InventoryItem pickup)
            {
                Pickup spawnedItem = pickup.SpawnPickup(item.PositionRotation.Position, item.CustomData[ECustomDataKey.StackSize] as int? ?? 1);
                spawnedItem.transform.SetParent(Parent.transform);
                spawnedItem.transform.localRotation = item.PositionRotation.Rotation;
            }
            
            
            if (item.CustomData != null)
            {
                if (mapObject is DestroyableProp destroyableProp)
                {
                    destroyableProp.SetHealth(item.CustomData[ECustomDataKey.Health] as float? ?? 1);
                }
                
                // ...
            }
        }
        
        public async Task SpawnItemsAsync(List<MapObjectConfiguration> items)
        {
            Task[] tasks = new Task[items.Count];
            
            foreach (MapObjectConfiguration item in items)
            {
                tasks[items.IndexOf(item)] = SpawnItemAsync(item);
            }
            
            await Task.WhenAll(tasks);
        }
        
        private async Task SpawnItemAsync(MapObjectConfiguration item)
        {
            SpawnItem(item);
            
            await Task.Yield();
        }
    }
}