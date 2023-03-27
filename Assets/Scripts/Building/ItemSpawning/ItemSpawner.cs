using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.InventoryManagement.Inventories;
using Scripts.InventoryManagement.Inventories.Items;
using Scripts.System;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using UnityEngine;
using static Scripts.Building.ItemSpawning.MapObjectConfiguration;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Building.ItemSpawning
{
    public class ItemSpawner : ISavable
    {
        private readonly Dictionary<int, GameObject> _spawnedObjects;
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        public static GameObject Parent => MapBuilder.ItemsParent;
        
        public ItemSpawner()
        {
            _spawnedObjects = new Dictionary<int, GameObject>();
            EventsManager.OnMapObjectRemovedFromMap.AddListener(OnInstanceRemovedFromMap);
        }

        public void SpawnItem(MapObjectConfiguration item)
        {
            MapObject mapObject = MapObject.GetFromID<MapObject>(item.ID);

            if (mapObject is InventoryItem pickup)
            {
                int stackSize = item.CustomData.TryGetValue(ECustomDataKey.StackSize, out object size) ? (int) size : 1;
                
                Pickup spawnedItem = pickup.SpawnPickup(item.PositionRotation.Position, stackSize);
                _spawnedObjects.Add(spawnedItem.GetInstanceID(), spawnedItem.gameObject);
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
            _spawnedObjects.Values.ForEach(item => item.DismissToPool());
            _spawnedObjects.Clear();
        }

        public string Guid { get; set; } = "ItemSpawner";
        
        public object CaptureState() => ItemInstancesToMapObjectConfigurations();

        public async void RestoreState(object state)
        {
            if (state is not List<MapObjectConfiguration> items)
            {
                Logger.LogWarning("State is not List<MapObjectConfiguration>".WrapInColor(Colors.Orange));
                return;
            }
            
            await SpawnItemsAsync(items);
        }
        
        public async Task RebuildItems()
        {
            DemolishItems();
            await SpawnItemsAsync(MapBuilder.MapDescription.MapObjects);
        }

        public List<MapObjectConfiguration> CollectMapObjects() => ItemInstancesToMapObjectConfigurations().ToList();

        private IEnumerable<MapObjectConfiguration> ItemInstancesToMapObjectConfigurations() 
            => _spawnedObjects.Values.Select(item => Create(item.GetComponent<MapObjectInstance>()));
        
        private async Task SpawnItemAsync(MapObjectConfiguration item)
        {
            await Task.Yield();
            
            SpawnItem(item);
        }
        
        private void OnInstanceRemovedFromMap(MapObjectInstance pickedUpItem)
        {
            _spawnedObjects.Remove(pickedUpItem.GetInstanceID());
        }
    }
}