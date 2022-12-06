using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.System
{
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController player;

        public PlayerController Player => player;
        public MapBuilder MapBuilder => mapBuilder;
        public MapDescription CurrentMap => _currentMap;
        public bool MovementEnabled => _movementEnabled;
        public EGameMode GameMode => _gameMode;

        private MapDescription _currentMap;
        private bool _movementEnabled;
        private EGameMode _gameMode = EGameMode.Play;

        private bool _startLevelAfterBuildFinishes;

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
            
            _startLevelAfterBuildFinishes = true;
            
            mapBuilder.BuildMap(_currentMap);
        }

        private void OnStartGameRequested()
        {
            _gameMode = EGameMode.Play;
            StartLevel(_currentMap ?? new MapDescription());
        }

        private void OnLayoutBuilt()
        {
            if (!_startLevelAfterBuildFinishes) return;
            player = ObjectPool.Instance.GetFromPool(playerPrefab.gameObject, Vector3.zero, Quaternion.identity)
                .GetComponent<PlayerController>();
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

        public void SetCurrentMap(MapDescription mapDescription)
        {
            mapBuilder.SetLayout(mapDescription.Layout);
            _currentMap = mapDescription;
        }
    }
}