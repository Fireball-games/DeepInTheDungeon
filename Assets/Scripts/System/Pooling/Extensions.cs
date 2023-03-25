using UnityEngine;

namespace Scripts.System.Pooling
{
    public static class Extensions
    {
        public static void DismissToPool(this GameObject returningObject) => ObjectPool.Instance.Dismiss(returningObject);
        
        public static T GetFromPool<T>(this T go, GameObject parent) where T : MonoBehaviour
        {
            return ObjectPool.Instance.Get(go.gameObject, parent).GetComponent<T>();
        }
    }
}