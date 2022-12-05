using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class Extensions
    {
        public static readonly Vector3Int Vector3IntZero;
        
        private static Vector3 _v3;
        private static Vector3Int _v3i;

        static Extensions()
        {
            _v3 = Vector3.zero;
            _v3i = Vector3Int.zero;
            Vector3IntZero = new(0, 0, 0);
        }
        
        public static Vector3 ToVector3(this Vector3Int source)
        {
            _v3.x = source.x;
            _v3.y = source.y;
            _v3.z = source.z;

            return _v3;
        }
        
        public static Vector3Int ToVector3Int(this Vector3 source)
        {
            _v3i.x = Mathf.RoundToInt(source.x);
            _v3i.y = Mathf.RoundToInt(source.y);
            _v3i.z = Mathf.RoundToInt(source.z);

            return _v3i;
        }

        public static void DestroyAllChildren(this GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        public static TKey GetFirstKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.FirstOrDefault(entry =>
                EqualityComparer<TValue>.Default.Equals(entry.Value, value)).Key;
        }
    }
}