using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

//Fireball Games * * * PetrZavodny.com

namespace Scripts.System.Pooling
{
    public class ObjectPool : SingletonNotPersisting<ObjectPool>
    {
        public List<PreSpawnSetItem> preSpawnSetItems;
        public Transform storeParent;
        public RectTransform uiParent;
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

        public GameObject GetFromPool(GameObject go, GameObject parent, bool isUiObject = false) => 
            GetFromPool(go, Vector3.zero, Quaternion.identity, parent, isUiObject);
        
        public GameObject GetFromPool(GameObject go, Vector3 position, Quaternion rotation, GameObject parent = null, bool isUiObject = false) 
        {
            if (Pool.ContainsKey(go.name) && Pool[go.name].Any())
            {
                GameObject removedObject = Pool[go.name].Dequeue();
                Transform removedObjectTransform = removedObject.transform;
                removedObjectTransform.position = position;
                removedObjectTransform.rotation = rotation;
                
                if (isUiObject)
                {
                    CheckUIParent();
                    removedObjectTransform.SetParent(parent ? parent.transform : uiParent);
                }
                else
                {
                    removedObjectTransform.parent = ResolveParentTransform(removedObjectTransform.name, parent);
                }
                
                removedObject.gameObject.SetActive(true);
                return ProcessInterfaces(removedObject);
            }
        
            return InstantiateNewPoolObject(go, position, rotation, parent, false, isUiObject);
        }

        public void ReturnToPool(GameObject returningObject, bool isUiObject = false)
        {
            if (!Pool.ContainsKey(returningObject.name))
            {
                Pool.Add(returningObject.name, new Queue<GameObject>());
            }

            Transform returningObjectTransform = returningObject.transform;
            returningObjectTransform.position = transform.position;
            
            if (isUiObject)
            {
                CheckUIParent();
                returningObjectTransform.SetParent(uiParent);
            }
            else
            {
                returningObjectTransform.parent = ResolveParentTransform(returningObject.name, null);
            }
            
            returningObject.gameObject.SetActive(false);
        
            Pool[returningObject.name].Enqueue(returningObject);
        }

        private GameObject InstantiateNewPoolObject(GameObject requestedObject, Vector3 position, Quaternion rotation, GameObject parent, bool instantiateToPoolStore = false, bool isUiObject = false)
        {
            Transform poolParent = parent ? parent.transform : storeParent;

            if (!parent && isUiObject)
            {
                CheckUIParent();
                poolParent = uiParent;
            }
            
            if (instantiateToPoolStore)
            {
                poolParent = ResolveParentTransform(requestedObject.name, parent);
            }

            if (!instantiateToPoolStore)
            {
                Logger.LogWarning($"Instantiating new {requestedObject.name}, consider pre create them instead.");
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

        private void CheckUIParent()
        {
            if (!uiParent)
            {
                throw new ArgumentException(
                    "Object Pool is asked to use UI parent, but such parent is not set. Can't continue.");
            }
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
            initializable?.Initialize();

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
