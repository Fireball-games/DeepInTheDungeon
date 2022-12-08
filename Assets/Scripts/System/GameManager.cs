using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.MapEditor;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.System
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController player;

        public PlayerController Player => player;
        public MapBuilder MapBuilder => mapBuilder;
        public MapDescription CurrentMap => _currentMap;
        public bool MovementEnabled => _movementEnabled;
        public bool IsPlayingFromEditor { get; set; }
        public EGameMode GameMode => _gameMode;

        private MapDescription _currentMap;
        private bool _movementEnabled;
        private EGameMode _gameMode = EGameMode.Play;

        private bool _startLevelAfterBuildFinishes;

        public enum EGameMode
        {
            MainMenu = 0,
            Play = 1,
            Editor = 2,
        }

        private void OnEnable()
        {
            EventsManager.OnStartGameRequested += OnStartGameRequested;
            EventsManager.OnSceneStartedLoading += OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading += OnSceneFinishedLoading;
            
            mapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void OnDisable()
        {
            EventsManager.OnStartGameRequested -= OnStartGameRequested;
            EventsManager.OnSceneStartedLoading -= OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading -= OnSceneFinishedLoading;
            
            mapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        private void StartBuildingLevel()
        {
            _gameMode = EGameMode.Play;
            
            _movementEnabled = false;

            _startLevelAfterBuildFinishes = true;

            _currentMap ??= MapBuilder.GenerateDefaultMap(3, 5, 5);
            
            mapBuilder.BuildMap(_currentMap);
        }

        private void OnStartGameRequested()
        {
            string loadSceneName = Scenes.PlayIndoorSceneName;
            
            if (_currentMap != null)
            {
                loadSceneName = _currentMap.SceneName;
            }
            
            SceneLoader.Instance.LoadScene(loadSceneName);
        }

        private void OnLayoutBuilt()
        {
            if (!_startLevelAfterBuildFinishes) return;
            
            player = ObjectPool.Instance.GetFromPool(playerPrefab.gameObject, Vector3.zero, Quaternion.identity)
                .GetComponent<PlayerController>();
            player.transform.parent = null;
            player.SetPositionAndRotation(_currentMap.StartPosition.ToVector3(), CurrentMap.PlayerRotation);
            player.SetCamera();
            
            _movementEnabled = true;
            EventsManager.TriggerOnLevelStarted();
        }

        private void OnSceneStartedLoading()
        {
            if (Player)
            {
                ObjectPool.Instance.ReturnToPool(Player.gameObject);
            }
        }

        private void OnSceneFinishedLoading(string sceneName)
        {
            MapBuilder.DemolishMap();
            
            if (sceneName is Scenes.EditorSceneName)
            {
                _startLevelAfterBuildFinishes = false;
                _gameMode = EGameMode.Editor;

                if (IsPlayingFromEditor)
                {
                    FindObjectOfType<MapEditorManager>().OrderMapConstruction(CurrentMap, true);
                }
                
                return;
            }

            if (sceneName is Scenes.MainSceneName)
            {
                IsPlayingFromEditor = false;
                CameraManager.Instance.SetMainCamera();
                _gameMode = EGameMode.MainMenu;
                return;
            }

            StartBuildingLevel();
        }

        public void SetCurrentMap(MapDescription mapDescription)
        {
            mapBuilder.SetLayout(mapDescription?.Layout);
            _currentMap = mapDescription;
        }
    }
}