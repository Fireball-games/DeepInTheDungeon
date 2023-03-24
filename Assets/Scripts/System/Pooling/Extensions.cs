using UnityEngine;

namespace Scripts.System.Pooling
{
    public static class Extensions
    {
        public static void DismissToPool(this GameObject returningObject) => ObjectPool.Instance.Dismiss(returningObject);
    }
}