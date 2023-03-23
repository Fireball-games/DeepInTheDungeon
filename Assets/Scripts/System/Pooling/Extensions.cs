using UnityEngine;

namespace Scripts.System.Pooling
{
    public static class Extensions
    {
        public static void Dismiss(this GameObject returningObject) => ObjectPool.Instance.Dismiss(returningObject);
    }
}