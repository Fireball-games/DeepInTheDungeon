using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.System.MonoBases;
using UnityEngine;

//Fireball Games * * * PetrZavodny.com

namespace Scripts.System.Pooling
{
    public class ObjectPool : SingletonNotPersisting<ObjectPool>
    {
        public List<PreSpawnSetItem> preSpawnSetItems;
        public Transform storeParent;
        private static readonly Dictionary<string, Queue<GameObject>> Pool = new();

        private Dictionary<string, Transform> _transforms;

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this) return;

            _transforms = new Dictionary<string, Transform>();
            
            if (preSpawnSetItems.Any())
            {
                SpawnPreSpawnItems();
            }
        }

        public GameObject GetFromPool(GameObject go, GameObject parent) => GetFromPool(go, Vector3.zero, Quaternion.identity, parent);
        
        public GameObject GetFromPool(GameObject go, Vector3 position, Quaternion rotation, GameObject parent = null) 
        {
            if (Pool.ContainsKey(go.name) && Pool[go.name].Any())
            {
                GameObject removedObject = Pool[go.name].Dequeue();
                Transform removedObjectTransform = removedObject.transform;
                removedObjectTransform.position = position;
                removedObjectTransform.rotation = rotation;
                
                if (removedObjectTransform is RectTransform)
                {
                    removedObjectTransform.SetParent(ResolveParentTransform(removedObjectTransform.name, parent));
                }
                else
                {
                    removedObjectTransform.parent = ResolveParentTransform(removedObjectTransform.name, parent);
                }
                
                removedObject.gameObject.SetActive(true);
                return ProcessInterfaces(removedObject);
            }
        
            return InstantiateNewPoolObject(go, position, rotation, parent);
        }

        public void ReturnToPool(GameObject returningObject)
        {
            if (!Pool.ContainsKey(returningObject.name))
            {
                Pool.Add(returningObject.name, new Queue<GameObject>());
            }

            Transform returningObjectTransform = returningObject.transform;
            returningObjectTransform.position = transform.position;
            
            if (returningObjectTransform is RectTransform)
            {
                returningObjectTransform.SetParent(ResolveParentTransform(returningObject.name, null));
            }
            else
            {
                returningObjectTransform.parent = ResolveParentTransform(returningObject.name, null);
            }
            
            returningObject.gameObject.SetActive(false);
        
            Pool[returningObject.name].Enqueue(returningObject);
        }

        private GameObject InstantiateNewPoolObject(GameObject requestedObject, Vector3 position, Quaternion rotation, GameObject parent, bool instantiateToPoolStore = false)
        {
            Transform poolParent = parent ? parent.transform : storeParent;

            if (instantiateToPoolStore)
            {
                poolParent = ResolveParentTransform(requestedObject.name, parent);
            }

            if (!instantiateToPoolStore)
            {
                // Logger.LogWarning($"Instantiating new {requestedObject.name}, consider pre create them instead.");
            }
            
            GameObject newObject = Instantiate(requestedObject, position, rotation, poolParent);
            newObject.name = requestedObject.name;
            newObject = ProcessInterfaces(newObject);
            
            if (instantiateToPoolStore)
            {
                newObject.SetActive(false);
            }
                
            return newObject;
        }

        private GameObject InstantiateNewPoolObject(GameObject objectToInstantiate, bool instantiateToPoolStore = false) 
        {
            Transform objectToInstantiateTransform = objectToInstantiate.transform;
            return InstantiateNewPoolObject(
                objectToInstantiate, objectToInstantiateTransform.position, objectToInstantiateTransform.rotation, gameObject, instantiateToPoolStore
            );
        }

        private GameObject ProcessInterfaces(GameObject target)
        {
            IPoolNeedy[] needy = target.GetComponents<IPoolNeedy>();
            if (needy.Any())
            { 
                foreach (IPoolNeedy poolNeedy in needy)
                {
                    if (!poolNeedy.pool)
                    {
                        poolNeedy.pool = this;
                    }
                }
            }

            IPoolInitializable initializable = target.GetComponent<IPoolInitializable>();
            initializable?.InitializeFromPool();

            return target;
        }
        
        private void SpawnPreSpawnItems()
        {
            preSpawnSetItems.ForEach(item =>
            {
                if (!item.prefabGameObject || item.howMany <= 0) return;

                for (int i = 0; i < item.howMany; i++)
                {
                    GameObject newObject = InstantiateNewPoolObject(item.prefabGameObject, true);
                    ReturnToPool(newObject);
                }
            });
        }

        private Transform ResolveParentTransform(string objectName, GameObject parent)
        {
            if (parent) return parent.transform;
            
            if (!_transforms.TryGetValue(objectName, out Transform result))
            {
                GameObject newParentGo = new(objectName)
                {
                    transform =
                    {
                        parent = storeParent
                    }
                };

                _transforms.Add(objectName, newParentGo.transform);

                result = newParentGo.transform;
            }

            return !result ? storeParent : result;
        }

        [Serializable]
        public class PreSpawnSetItem
        {
            public GameObject prefabGameObject;
            public int howMany;
        }
    }
}
