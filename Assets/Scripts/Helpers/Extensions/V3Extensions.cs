using System;
using System.Collections.Generic;
using UnityEngine;
using static Scripts.Helpers.Extensions.GeneralExtensions;

namespace Scripts.Helpers.Extensions
{
    public static class V3Extensions
    {
        public static Dictionary<Vector3, Quaternion> DirectionRotationMap;

        static V3Extensions()
        {
            DirectionRotationMap = new Dictionary<Vector3, Quaternion>
            {
                { WorldNorth, Quaternion.Euler(Vector3.zero) },
                { WorldEast, Quaternion.Euler(new Vector3(0, 90, 0)) },
                { WorldSouth, Quaternion.Euler(Vector3.zero) },
                { WorldWest, Quaternion.Euler(new Vector3(0, 90, 0)) },
            };
        }

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
            V3I.x = Mathf.RoundToInt(-source.y);
            V3I.y = Mathf.RoundToInt(source.x);
            V3I.z = Mathf.RoundToInt(source.z);

            return V3I;
        }

        public static Vector3 Round(this Vector3 source, int decimalNumbers)
        {
            V3.x = (float)Math.Round(source.x, decimalNumbers);
            V3.y = (float)Math.Round(source.y, decimalNumbers);
            V3.z = (float)Math.Round(source.z, decimalNumbers);

            return new Vector3(V3.x, V3.y, V3.z);
        }
    }
}