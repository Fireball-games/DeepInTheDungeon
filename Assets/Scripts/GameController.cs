using System.Collections.Generic;
using Scripts.Building;
using Scripts.Building.Tile;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts
{
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController player;

        public static TileDescription[,] CurrentMapLayout => _currentMap.Layout;
        public static bool MovementEnabled => _movementEnabled;
        public static EGameMode GameMode => _gameMode;

        private static MapDescription _currentMap;
        private static bool _movementEnabled;
        private static EGameMode _gameMode = EGameMode.Play;

        public enum EGameMode
        {
            Play = 1,
            Editor = 2,
        }

        private void OnEnable()
        {
            EventsManager.OnStartGameRequested += OnStartGameRequested;
            EventsManager.OnSceneFinishedLoading += OnSceneFinishedLoading;
            
            mapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnDisable()
        {
            EventsManager.OnStartGameRequested -= OnStartGameRequested;
            EventsManager.OnSceneFinishedLoading -= OnSceneFinishedLoading;
            
            mapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        private void StartLevel(MapDescription map)
        {
            _movementEnabled = false;
            _currentMap = map;
            mapBuilder.BuildMap(_currentMap);
            
        }

        private void OnStartGameRequested()
        {
            _gameMode = EGameMode.Play;
            StartLevel(_currentMap ?? new MapDescription());
        }

        private void OnLayoutBuilt()
        {
            player = Instantiate(playerPrefab);
            player.SetPosition(_currentMap.StartPosition.ToVector3());
            _movementEnabled = true;
            EventsManager.TriggerOnLevelStarted();
        }

        private void OnSceneFinishedLoading(string sceneName)
        {
            if (sceneName == Scenes.EditorSceneName)
            {
                _gameMode = EGameMode.Editor;
            }

            if (sceneName == Scenes.MainSceneName)
            {
                _gameMode = EGameMode.Play;
            }
        }
    }
}