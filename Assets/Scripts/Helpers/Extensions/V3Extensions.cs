using UnityEngine;
using static Scripts.Helpers.GeneralExtensions;

namespace Scripts.Helpers.Extensions
{
    public static class V3Extensions
    {
        public static Vector3 SwapXY(this Vector3 source)
        {
            (source.y, source.x) = (source.x, source.y); // Swapping via deconstruction, noice
            return source;
        }
        
        public static Vector3Int ToVector3Int(this Vector3 source)
        {
            V3I.x = Mathf.RoundToInt(source.x);
            V3I.y = Mathf.RoundToInt(source.y);
            V3I.z = Mathf.RoundToInt(source.z);

            return V3I;
        }
        
        public static Vector3Int ToGridPosition(this Vector3 source)
        {
            V3I.x = Mathf.RoundToInt(source.y);
            V3I.y = Mathf.RoundToInt(-source.x);
            V3I.z = Mathf.RoundToInt(source.z);

            return V3I;
        }
    }
}