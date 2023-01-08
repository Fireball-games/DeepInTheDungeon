using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Helpers.Extensions
{
    public static class GeneralExtensions
    {
        public static readonly Vector3Int WorldUp;
        public static readonly Vector3Int WorldDown;
        public static readonly Vector3Int WorldNorth;
        public static readonly Vector3Int WorldEast;
        public static readonly Vector3Int WorldSouth;
        public static readonly Vector3Int WorldWest;

        public static readonly Vector3Int GridNorth;
        public static readonly Vector3Int GridEast;
        public static readonly Vector3Int GridSouth;
        public static readonly Vector3Int GridWest;

        public static Vector3 V3;
        public static Vector3Int V3I;

        static GeneralExtensions()
        {
            V3 = Vector3.zero;
            V3I = Vector3Int.zero;
            // TODO: WorldUp nad WorldDown are weird, should be other way around, but I'm afraid to change it to break something, check when possible
            WorldUp = Vector3Int.down;
            WorldDown = Vector3Int.up;
            WorldNorth = Vector3Int.left;
            WorldEast = Vector3Int.forward;
            WorldSouth = Vector3Int.right;
            WorldWest = Vector3Int.back;

            GridNorth = new Vector3Int(0, -1, 0);
            GridEast = new Vector3Int(0, 0, 1);
            GridSouth = new Vector3Int(0, 1, 0);
            GridWest = new Vector3Int(0, 0, -1);
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

        public static T ByGridV3Int<T>(this List<List<List<T>>> source, Vector3Int gridPosition)
        {
            return source[gridPosition.x][gridPosition.y][gridPosition.z];
        }

        public static T ByGridV3Int<T>(this T[,,] source, Vector3Int position)
        {
            return source[position.x, position.y, position.z];
        }

        public static bool HasIndex<T>(this List<List<List<T>>> source, Vector3Int gridPosition)
            => source.HasIndex(gridPosition.x, gridPosition.y, gridPosition.z);

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

        public static Transform GetBody(this GameObject source) => source.transform.Find("Body");

        public static TKey GetFirstKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.FirstOrDefault(entry =>
                EqualityComparer<TValue>.Default.Equals(entry.Value, value)).Key;
        }

        public static Color SetIntensity(this Color source, float intensity)
        {
            Color result = new()
            {
                r = source.r * intensity,
                g = source.g * intensity,
                b = source.b * intensity
            };
            return result;
        }

        public static Color Clone(this Color source) => new(source.r, source.g, source.b, source.a);

        public static void CreateDirectoryIfNotExists(this string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static GameObject FindActive(this Transform transform, string name)
        {
            foreach (Transform child in transform)
            {
                GameObject foundChild = child.gameObject;
                if (foundChild.name == name && foundChild.activeSelf)
                {
                    return foundChild;
                }
            }

            return null;
        }

        public static void SetTextColor(this Button button, Color color) => button.GetComponentInChildren<TMP_Text>().color = color;
    }
}