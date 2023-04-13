using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Helpers.Extensions
{
    public static class GoExtensions
    {
        public static void DestroyAllChildren(this GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static void DismissAllChildrenToPool(this GameObject go)
        {
            while (go.transform.childCount > 0)
            {
                // TODO: seems sometimes not returning all the kids, check it out
                // Logger.Log("Shooing kids to pool");
                foreach (Transform child in go.transform)
                {
                    ObjectPool.Instance.Dismiss(child.gameObject);
                }
            }
        }

        public static Transform GetBody(this GameObject source) => source.transform.Find("Body");
        
        public static bool TryGetComponent<TComponent>(this GameObject source, out TComponent outComponent, bool addIfMissing = false) where TComponent : Component
        {
            outComponent = source.GetComponent<TComponent>();
            
            if (outComponent == null)
            {
                if (addIfMissing)
                {
                    outComponent = source.AddComponent<TComponent>();
                    return true;
                }
                
                Logger.LogWarning($"No {typeof(TComponent).Name} found on {source.name}".WrapInColor(Colors.Orange));
            }
            
            return outComponent;
        }
    }
}