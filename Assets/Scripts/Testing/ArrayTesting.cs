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

            string result = $"SizeX: {arr.GetLength(0)}, SizeY: {arr.GetLength(1)}\n";
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result += $"| {x}:{y}:{arr[x, y]} |";
                    
                }
                
                result += "\n";
                
            }
        
            Debug.Log(result);
        }
    }
}
