using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts.Helpers.Attributes;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;
using Object = UnityEngine.Object;

namespace Scripts.System
{
    /// <summary>
    /// Base class for using InstantiateFromResource and AssignFromResource attributes to automatically instantiate
    /// GameObject for MonoBehaviour or assign a field from loaded prefab respectively.
    /// 
    /// *****************************************************
    /// ***             InstantiateFromResource           ***
    /// *****************************************************
    /// 
    /// [InstantiateFromResource("Prefabs/cube")]
    /// public class BlaBla() : MonoBehaviour ...
    /// Loads prefab and instantiate it for gameObject this script is on. LocalPosition 0,0,0 and sameRotation as the prefab.
    ///
    /// [InstantiateFromResource("Prefabs/cube", typeof(TestScript))]
    /// public class BlaBla() : MonoBehaviour ...
    /// Loads prefab and instantiate it for gameObject this script is on. LocalPosition 0,0,0 and sameRotation as the prefab.
    /// Also tests if loaded prefab has MonoBehaviour of required type.
    ///
    /// [InstantiateFromResource("Prefabs/cube", typeof(TestScript), new[] {2f, 2f, 2f})]
    /// public class BlaBla() : MonoBehaviour ...
    /// Loads prefab and instantiate it for gameObject this script is on. LocalPosition is specified in 3rd parameter,
    /// rotation remains the same like on the prefab.
    /// Also tests if loaded prefab has MonoBehaviour of required type. Can use "MonoBehaviour" to skip this check.
    ///
    /// [InstantiateFromResource("Prefabs/cube", typeof(TestScript), new[] {2f, 2f, 2f}, true, new[] {0f, 180f, 0f)]
    /// public class BlaBla() : MonoBehaviour ...
    /// Loads prefab and instantiate it for gameObject this script is on. LocalPosition is specified in 3rd parameter,
    /// If overwriteRotation parameter is true, then rotation will be overwritten with default Vector3.zero or by
    /// rotation specified in "rotation" parameter. With overwriteRotation set to false (by default) rotation remains
    /// the same like on prefab.
    /// Also tests if loaded prefab has MonoBehaviour of required type. Can use "MonoBehaviour" to skip this check. 
    ///
    /// *****************************************************
    /// ***               AssignFromResource              ***
    /// *****************************************************
    /// 
    /// Usage on fields (both private and public can be used):
    /// 
    /// [AssignFromResource("Prefabs/cube")]
    /// Loads prefab from resources and assign it to the field.
    ///
    /// [AssignFromResource("Prefabs/cube", typeof(TestScript))]
    /// Loads prefab from resources, tests if the prefab includes desired script and if so, then assign it to the field.
    ///
    /// *****************************************************
    /// ***                  More Info                    ***
    /// *****************************************************
    /// 
    /// In case, that inheriting from this class is not convenient, you can call ProcessAttributes
    /// static method from Awake to do the same thing.  
    /// </summary>
    public abstract class InitializeFromResourceBase : MonoBehaviour
    {
        private static Type _targetType;
        private static Object _targetObject;
    
        protected virtual void Awake()
        {
            _targetObject = this;
            _targetType = GetType();
            InstantiateForClass();
        
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            AssignPrefabsToFields(fields);
        }

        private static void InstantiateForClass()
        {
            IEnumerable<InstantiateFromResourceAttribute> classAttrs = _targetType.GetCustomAttributes(typeof(InstantiateFromResourceAttribute))
                .Select(attribute => (InstantiateFromResourceAttribute)attribute);

            foreach (InstantiateFromResourceAttribute attribute in classAttrs)
            {
                if (!LoadPrefabFromResourceChecked(attribute.SourcePath, out GameObject prefab)) return;

                if (!CheckCorrectComponent(prefab, attribute.RequiredPrefabType)) return;
            
                GameObject newInstance = Instantiate(prefab, ((MonoBehaviour)_targetObject).transform);
                newInstance.transform.localPosition = attribute.Position;
            
                if (attribute.OverwriteRotation)
                {
                    newInstance.transform.localRotation = Quaternion.Euler(attribute.Rotation);
                }
            }
        }

        private static void AssignPrefabsToFields(FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (!(Attribute.GetCustomAttribute(field, typeof(AssignFromResourceAttribute), false) is
                        AssignFromResourceAttribute attribute)) continue;

                if (!LoadFromResourceChecked(attribute.SourcePath, field.FieldType, out Object loadedObject)) return;

                if(!CheckCorrectComponent(loadedObject, attribute.RequiredPrefabType)) return;
            
                field.SetValue(_targetObject, loadedObject);
            }
        }
    
        private static bool LoadFromResourceChecked(string path, Type fieldType, out Object loadedPrefab)
        {
            Object loadedObject = Resources.Load(path, fieldType);

            if (loadedObject != null)
            {
                loadedPrefab = loadedObject;
                return true;
            }
        
            Logger.LogError($"Prefab was not loaded on given path: {path}.");
            loadedPrefab = null;
            return false;
        }
    
        private static bool LoadPrefabFromResourceChecked(string path, out GameObject loadedPrefab)
        {
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab)
            {
                loadedPrefab = prefab;
                return true;
            }
        
            Logger.LogError($"Prefab was not loaded on given path: {path}.");
            loadedPrefab = null;
            return false;
        }

        private static bool CheckCorrectComponent(Object testedObject, Type requiredType)
        {
            if (testedObject.GetType() == requiredType) return true;
        
            if (!requiredType.IsSubclassOf(typeof(Component)))
            {
                Logger.LogError($"Required type: \"{requiredType}\" on loaded object is not inheriting from Component class");
                return false;
            }
        
            if (requiredType != typeof(MonoBehaviour) 
                && testedObject is GameObject go && !go.GetComponent(requiredType))
            {
                Logger.LogError($"Loaded object does not contain required component \"{requiredType}\".");
                return false;
            }

            return true;
        }

        public static void ProcessAttributes<T>(T target) where T : MonoBehaviour
        {
            _targetObject = target;
            _targetType = typeof(T);
        
            InstantiateForClass();
        
            FieldInfo[] fields = _targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            AssignPrefabsToFields(fields);
        }
    }
}