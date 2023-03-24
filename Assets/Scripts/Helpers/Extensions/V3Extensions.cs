using System;
using System.Collections.Generic;
using UnityEngine;
using static Scripts.Helpers.Extensions.GeneralExtensions;

namespace Scripts.Helpers.Extensions
{
    public static class V3Extensions
    {
        public static readonly Dictionary<Vector3, Quaternion> WallDirectionRotationMap;
        public static readonly Dictionary<Vector3, Vector3> DirectionRotationMap;
        
        public static readonly Vector3 Positive90InY= new(0, 90f, 0);
        public static readonly Vector3 Negative90InY = new(0, -90f, 0);
        public static readonly Vector3 Zero = new(0, 0, 0);

        static V3Extensions()
        {
            WallDirectionRotationMap = new Dictionary<Vector3, Quaternion>
            {
                { WorldNorth, Quaternion.Euler(Vector3.zero) },
                { WorldEast, Quaternion.Euler(new Vector3(0, 90, 0)) },
                { WorldSouth, Quaternion.Euler(new Vector3(0, 180, 0)) },
                { WorldWest, Quaternion.Euler(new Vector3(0, 270, 0)) },
            };

            DirectionRotationMap = new Dictionary<Vector3, Vector3>
            {
                {WorldNorth, new Vector3(0, 270, 0)},
                {WorldEast, Vector3.zero},
                {WorldSouth, new Vector3(0, 90, 0)},
                {WorldWest, new Vector3(0, 180, 0)},
            };
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

        public static Vector3 Round(this Vector3 source, int decimalPlaces)
        {
            float multiplier = (float)Math.Pow(10.0f, decimalPlaces);
            source.x = (float)Math.Round(source.x * multiplier, MidpointRounding.AwayFromZero) / multiplier;
            source.y = (float)Math.Round(source.y * multiplier, MidpointRounding.AwayFromZero) / multiplier;
            source.z = (float)Math.Round(source.z * multiplier, MidpointRounding.AwayFromZero) / multiplier;
            return source;
        }
    }
}