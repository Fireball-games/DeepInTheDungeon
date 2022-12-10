using System.Collections.Generic;
using System.Linq;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class Extensions
    {
        public static readonly Vector3Int GridUp;
        public static readonly Vector3Int GridDown;
        public static readonly Vector3Int GridNorth;
        public static readonly Vector3Int GridEast;
        public static readonly Vector3Int GridSouth;
        public static readonly Vector3Int GridWest;

        private static Vector3 _v3;
        private static Vector3Int _v3I;

        static Extensions()
        {
            _v3 = Vector3.zero;
            _v3I = Vector3Int.zero;
            GridUp = Vector3Int.down;
            GridDown = Vector3Int.up;
            GridNorth = Vector3Int.left;
            GridEast = Vector3Int.forward;
            GridSouth = Vector3Int.right;
            GridWest = Vector3Int.back;
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
            _v3I.x = Mathf.RoundToInt(source.x);
            _v3I.y = Mathf.RoundToInt(source.y);
            _v3I.z = Mathf.RoundToInt(source.z);

            return _v3I;
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

        public static bool HasIndex<T>(this T[,,] source, Vector3Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < source.GetLength(0)
                                       && gridPosition.y >= 0 && gridPosition.y < source.GetLength(1)
                                       && gridPosition.z >= 0 && gridPosition.z < source.GetLength(2);
        }

        public static bool HasIndex<T>(this List<List<List<T>>> source, int floor, int row, int column)
        {
            return floor >= 0 && floor < source.Count
                              && row >= 0 && row < source[0].Count
                              && column >= 0 && column < source[0][0].Count;
        }

        public static T ByGridV3int<T>(this T[,,] source, Vector3Int position)
        {
            return source[position.x, position.y, position.z];
        }

        public static Vector3 SwapXY(this Vector3 source)
        {
            (source.y, source.x) = (source.x, source.y); // Swapping via deconstruction, noice
            return source;
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

        public static bool HasIndex<T>(this List<List<T>> source, int row, int column)
        {
            return row >= 0 && row < source.Count && column >= 0 && column < source[0].Count;
        }

        public static void DestroyAllChildren(this GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static void DismissAllChildrenToPool(this GameObject go, bool isUiObject = false)
        {
            while (go.transform.childCount > 0)
            {
                // TODO: seems sometimes not returning all the kids, check it out
                // Logger.Log("Shooing kids to pool");
                foreach (Transform child in go.transform)
                {
                    ObjectPool.Instance.ReturnToPool(child.gameObject, isUiObject);
                }
            }
        }

        public static TKey GetFirstKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.FirstOrDefault(entry =>
                EqualityComparer<TValue>.Default.Equals(entry.Value, value)).Key;
        }
    }
}