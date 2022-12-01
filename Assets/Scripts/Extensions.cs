using UnityEngine;

namespace Scripts
{
    public static class Extensions
    {
        private static Vector3 _v3 = new Vector3();
        
        public static Vector3 ToVector3(this Vector3Int source)
        {
            _v3.x = source.x;
            _v3.y = source.y;
            _v3.z = source.z;

            return _v3;
        }
    }
}