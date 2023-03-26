using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Inventory.Inventories;
using Scripts.Inventory.Inventories.Items;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using UnityEngine;
using static Scripts.Building.ItemSpawning.MapObjectConfiguration;

namespace Scripts.Building.ItemSpawning
{
    public class ItemSpawner : ISavable
    {
        private readonly Dictionary<int, GameObject> _spawnedItems;
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        public static GameObject Parent => MapBuilder.ItemsParent;
        
        public ItemSpawner()
        {
            _spawnedItems = new Dictionary<int, GameObject>();
        }
        
        public void SpawnItem(MapObjectConfiguration item)
        {
            MapObject mapObject = MapObject.GetFromID<MapObject>(item.ID);

            if (mapObject is InventoryItem pickup)
            {
                int stackSize = item.CustomData.TryGetValue(ECustomDataKey.StackSize, out object size) ? (int) size : 1;
                
                Pickup spawnedItem = pickup.SpawnPickup(item.PositionRotation.Position, stackSize);
                _spawnedItems.Add(spawnedItem.GetInstanceID(), spawnedItem.gameObject);
                spawnedItem.SetTransform(item.PositionRotation);
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

        public void DemolishItems()
        {
            _spawnedItems.Values.ForEach(item => item.DismissToPool());
            _spawnedItems.Clear();
        }

        public string Guid { get; set; } = "ItemSpawner";
        
        public object CaptureState() => ItemInstancesToMapObjectConfigurations();

        public async void RestoreState(object state)
        {
            if (state is not List<MapObjectConfiguration> items)
            {
                Helpers.Logger.LogWarning("State is not List<MapObjectConfiguration>".WrapInColor(Colors.Orange));
                return;
            }
            
            await SpawnItemsAsync(items);
        }

        public List<MapObjectConfiguration> CollectMapObjects() => ItemInstancesToMapObjectConfigurations().ToList();

        private IEnumerable<MapObjectConfiguration> ItemInstancesToMapObjectConfigurations() 
            => _spawnedItems.Values.Select(item => Create(item.GetComponent<MapObjectInstance>()));
        
        private async Task SpawnItemAsync(MapObjectConfiguration item)
        {
            await Task.Yield();
            
            SpawnItem(item);
        }

        public async Task RebuildItems()
        {
            DemolishItems();
            await SpawnItemsAsync(MapBuilder.MapDescription.MapObjects);
        }
    }
}