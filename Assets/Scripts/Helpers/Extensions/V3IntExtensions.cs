using UnityEngine;
using static Scripts.Helpers.GeneralExtensions;

namespace Scripts.Helpers.Extensions
{
    public static class V3IntExtensions
    {
        public static Vector3 ToVector3(this Vector3Int source)
        {
            V3.x = source.x;
            V3.y = source.y;
            V3.z = source.z;

            return V3;
        }
        
        public static Vector3 ToWorldPosition(this Vector3Int gridPosition)
        {
            gridPosition = gridPosition.SwapXY();
            gridPosition.y = -gridPosition.y;
            return gridPosition;
        }
        
        public static Vector3Int ToWorldPositionV3Int(this Vector3Int gridPosition)
        {
            gridPosition = gridPosition.SwapXY();
            gridPosition.y = -gridPosition.y;
            return gridPosition;
        }
        
        public static Vector3Int SwapXY(this Vector3Int source)
        {
            (source.y, source.x) = (source.x, source.y);
            return source;
        }

        public static Vector3Int AddToX(this Vector3Int source, int value)
        {
            source.x += value;
            return source;
        }
    }
}