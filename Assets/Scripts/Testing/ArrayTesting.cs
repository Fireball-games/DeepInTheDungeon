using UnityEngine;

namespace Scripts.Testing
{
    public class ArrayTesting : MonoBehaviour
    {
        private void Start()
        {
            int width = 5;
            int height = 7;
            string[,] arr = new string[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    arr[x, y] = $"{x}, {y}";
                }
            }

            string result = "";
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result += $"| {x}:{y}:{arr[x, y]} |";
                }
            }
        
            Debug.Log(result);
        }
    }
}
