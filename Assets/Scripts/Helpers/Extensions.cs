using UnityEngine;

namespace Scripts.Helpers
{
    public static class Extensions
    {
        private static Vector3 _v3;

        static Extensions()
        {
            _v3 = Vector3.zero;
        }
        
        public static Vector3 ToVector3(this Vector3Int source)
        {
            _v3.x = source.x;
            _v3.y = source.y;
            _v3.z = source.z;

            return _v3;
        }

        public static void DestroyAllChildren(this GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}