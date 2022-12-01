using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController player;

        public static List<List<int>> CurrentMapLayout => _currentMap.Layout;

        private static MapDescription _currentMap;
        
        private void Start()
        {
            _currentMap = new();
            mapBuilder.BuildMap(_currentMap);
            player.SetPosition(_currentMap.StartPosition.ToVector3());
        }
    }
}