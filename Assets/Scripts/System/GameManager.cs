using System;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.Triggers;
using Scripts.UI;
using Scripts.UI.EditorUI;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameConfiguration gameConfiguration;
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController _player;
        
        private Campaign _mainCampaign;
        private Campaign _startRoomsCampaign;

        public GameConfiguration GameConfiguration => gameConfiguration;
        public PlayerController Player => _player;
        public Vector3Int PlayerPosition => Player.transform.position.ToVector3Int();
        public MapBuilder MapBuilder => mapBuilder;
        /// <summary>
        /// Current campaign is set only in GameManager and is pointing to LastEditedCampaign, StartRooms, SelectedCampaign or Loaded campaign. 
        /// </summary>
        public Campaign CurrentCampaign => _currentCampaign;
        public MapDescription CurrentMap => _currentMap;
        public bool MovementEnabled => _movementEnabled;
        public bool IsPlayingFromEditor { get; set; }
        public EGameMode GameMode => _gameMode;

        private PlayerCameraController PlayerCamera => PlayerCameraController.Instance;

        private Campaign _currentCampaign;
        private MapDescription _currentMap;
        private EntryPoint _currentEntryPoint;
        private bool _movementEnabled;
        private EGameMode _gameMode = EGameMode.Play;

        private bool _lookModeOnStartTraversal;
        private bool _startLevelAfterBuildFinishes;
        private bool _isPlayingFromSavedGame;
        private bool _entryMovementFinished;

        public enum EGameMode
        {
            MainScene = 0,
            Play = 1,
            Editor = 2,
        }

        private void OnEnable()
        {
            EventsManager.OnSceneStartedLoading += OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading += OnSceneFinishedLoading;
            EventsManager.OnMapTraversalTriggered += OnMapTraversalTriggered;

            if (MapBuilder)
            {
                mapBuilder.OnLayoutBuilt.AddListener(OnLayoutBuilt);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (!FileOperationsHelper.LoadSystemCampaigns(out _mainCampaign, out _startRoomsCampaign))
            {
                Logger.LogError("Failed to load system campaigns.");
            }
            
            _currentEntryPoint = new EntryPoint();
        }

        private void OnDisable()
        {
            EventsManager.OnSceneStartedLoading -= OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading -= OnSceneFinishedLoading;
            EventsManager.OnMapTraversalTriggered -= OnMapTraversalTriggered;

            if (MapBuilder)
            {
                mapBuilder.OnLayoutBuilt.RemoveListener(OnLayoutBuilt);
            }
        }

        public void SetCurrentCampaign(Campaign campaign)
        {
            _currentCampaign = campaign;
        }

        public void SetCurrentMap(MapDescription mapDescription)
        {
            if (mapDescription  == null) return;
            
            _currentMap = mapDescription;
            
            mapBuilder.SetLayout(mapDescription.Layout);
        }
        
        public void StartMainScene(bool fadeIn = true)
        {
            // _currentCampaign = FileOperationsHelper.LoadStartRoomsCampaign();
            _currentCampaign = _startRoomsCampaign;
            
            bool startedMapBoolFailed = false;
            
            if (_currentCampaign == null)
            {
                startedMapBoolFailed = true;
                Logger.LogError("Could not load start rooms campaign.");
                _currentCampaign = MapBuilder.GenerateFallbackStartRoomsCampaign();
            }
            
            //TODO: Here will be logic determining which start room to load depending on player progress. Its StarterMap for now.
            
            _currentMap = _currentCampaign.GetStarterMap();
            if (_currentMap.EntryPoints.Count > 0)
            {
                _currentEntryPoint = _currentMap.EntryPoints[0].Cloned();
            }
            else if (startedMapBoolFailed)
            {
                Logger.LogError("Could not load entry point.");
                _currentEntryPoint = new EntryPoint
                {
                    isMovingForwardOnStart = false,
                    playerGridPosition = _currentMap.EditorStartPosition,
                    playerRotationY = (int)_currentMap.EditorPlayerStartRotation.eulerAngles.y
                };
            }

            if (_currentCampaign == null || _currentMap == null)
            {
                Logger.LogError("Could not load last played campaign.");
                return;
            }
            
            OnStartGameRequested(fadeIn);
        }

        public async void StartNewCampaign()
        {
            // TODO: When applicable, handle warning about deleting save files.
            _currentCampaign = _mainCampaign;
            _currentMap = _currentCampaign.GetStarterMap();
            _currentEntryPoint = _currentMap.EntryPoints[0].Cloned();
            
            PlayerCamera.IsLookModeOn = false;
            
            _player.PlayerMovement.MoveForward(true);
            FindObjectOfType<DoTweenTriggerReceiver>().Trigger();
            
            await Task.Delay(2500);
            
            OnStartGameRequested();
        }
        
        public void ContinueFromSave()
        {
            // TODO: Gets data from saved position. 
            _currentCampaign = FileOperationsHelper.LoadLastPlayedCampaign();
            _currentMap = _currentCampaign.GetStarterMap();
            // TODO: Once save positions work, get data from there
            _currentEntryPoint.playerGridPosition = _currentMap.EditorStartPosition;
            _currentEntryPoint.playerRotationY = (int)_currentMap.EditorPlayerStartRotation.eulerAngles.y;
            _currentEntryPoint.isMovingForwardOnStart = false;
            
            if (_currentCampaign == null || _currentMap == null)
            {
                Logger.LogError("Could not load last played campaign.");
                return;
            }
            
            OnStartGameRequested();
        }

        public void LoadLastEditedMap(EntryPoint entryPoint = null)
        {
            if (!FileOperationsHelper.GetLastEditedCampaignAndMap(out _currentCampaign, out _currentMap))
            {
                return;
            }

            if (entryPoint == null)
            {
                _currentEntryPoint.playerGridPosition = _currentMap.EditorStartPosition;
                _currentEntryPoint.playerRotationY = (int)_currentMap.EditorPlayerStartRotation.eulerAngles.y;
                _currentEntryPoint.isMovingForwardOnStart = false;
            }
            else
            {
                _currentEntryPoint = entryPoint.Cloned();
            }
            
            OnStartGameRequested();
        }

        private async void OnMapTraversalTriggered(string exitConfigurationGuid)
        {
            if (CurrentMap == null || !_entryMovementFinished) return;
            
            TriggerConfiguration mapTraversal = TriggerService.GetConfiguration(exitConfigurationGuid);
            
            if (mapTraversal == null)
            {
                Logger.LogWarning($"Could not find exit point trigger configuration with guid {exitConfigurationGuid}.");
                return;
            }
            // Storing so if getting the data fails, game can continue.
            MapDescription mapDescription = _currentCampaign.GetMapByName(mapTraversal.TargetMapName);
            
            if (mapDescription == null)
            {
                Logger.LogWarning($"Could not find map with name {mapTraversal.TargetMapName}.");
                return;
            }
            
            EntryPoint entryPoint = mapDescription.GetEntryPointCloneByName(mapTraversal.TargetMapEntranceName);
            
            if (entryPoint == null)
            {
                Logger.LogWarning($"Could not find entry point in map: {mapTraversal.TargetMapName.WrapInColor(Colors.Warning)} with name: {mapTraversal.TargetMapEntranceName.WrapInColor(Colors.Warning)}.");
                return;
            }
            
            _currentMap = mapDescription;
            _currentEntryPoint = entryPoint;
            
            _lookModeOnStartTraversal = PlayerCamera.IsLookModeOn;
            PlayerCamera.IsLookModeOn = false;
            
            await Task.Delay((int)mapTraversal.ExitDelay * 1000);
            
            OnStartGameRequested();
        }
        

        private void OnStartGameRequested(bool fadeIn = true)
        {
            SceneLoader.Instance.LoadScene(_currentMap.SceneName, fadeIn, 1f);
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
                
                ScreenFader.FadeOut(0.5f);
                
                if (IsPlayingFromEditor)
                {
                    MapEditorManager.Instance.OrderMapConstruction(CurrentMap, true);
                }
                else
                {
                    EditorUIManager.Instance.ShowEditorUI();
                }
                
                return;
            }

            if (sceneName is Scenes.MainSceneName)
            {
                // IsPlayingFromEditor = false;
                if (!IsPlayingFromEditor)
                {
                    _gameMode = EGameMode.MainScene;
                }
            }

            StartBuildingLevel();
        }
        
        private void StartBuildingLevel()
        {
            if (_currentCampaign == null)
            {
                Logger.LogWarning("No Campaign is set, this should not happen here, ever.");
                return;
            }
            
            _gameMode = EGameMode.Play;
            
            _movementEnabled = false;

            _startLevelAfterBuildFinishes = true;
            
            _currentMap ??= _currentCampaign.GetStarterMap();

            PlayerPrefs.SetString(Strings.LastPlayedCampaign, _currentCampaign.CampaignName);
            
            mapBuilder.BuildMap(_currentMap);
        }

        private async void OnLayoutBuilt()
        {
            if (!_startLevelAfterBuildFinishes) return;
            
            _player = ObjectPool.Instance.GetFromPool(playerPrefab.gameObject, Vector3.zero, Quaternion.identity)
                .GetComponent<PlayerController>();
            _player.transform.parent = null;
            _player.PlayerMovement.SetPositionAndRotation(
                _currentEntryPoint.playerGridPosition,
                Quaternion.Euler(0f, _currentEntryPoint.playerRotationY, 0f));
            _player.PlayerMovement.SetCamera();
            
            // To allow playing StartRooms from Editor
            _movementEnabled = true;

            ScreenFader.FadeOut(1.2f);

            await Task.Delay(200);
            
            if (_currentEntryPoint.isMovingForwardOnStart)
            {
                _movementEnabled = false;
                _entryMovementFinished = false;
                
                if (SceneLoader.IsInMainScene && !IsPlayingFromEditor)
                {
                    HandleEntryMovement(SetControlsForMainScene);
                }
                else
                {
                    HandleEntryMovement( () =>
                    {
                        _movementEnabled = true;
                        PlayerCamera.IsLookModeOn = _lookModeOnStartTraversal;
                    });
                }
                
                _player.PlayerMovement.MoveForward(true);
            }
            
            EventsManager.TriggerOnLevelStarted();
        }
        
        private Action _onMovementFinished;
        private void HandleEntryMovement(Action onMovementFinished)
        {
            if (onMovementFinished != null) _onMovementFinished = onMovementFinished;
            PlayerMovement.OnStartResting.AddListener(OnMovementFinishedWrapper);
        }
        
        private void OnMovementFinishedWrapper()
        {
            _onMovementFinished?.Invoke();
            _onMovementFinished = null;
            _entryMovementFinished = true;
            PlayerMovement.OnStartResting.RemoveListener(OnMovementFinishedWrapper);
        }

        private void SetControlsForMainScene()
        {
            _movementEnabled = false;
            PlayerCameraController.Instance.IsLookModeOn = true;
            PlayerCameraController.Instance.SetRotationLimits(new RotationSettings
            {
                MinXRotation = -60f,
                MaxXRotation = 60f,
                MinYRotation = -85f,
                MaxYRotation = 85f
            });
            MainUIManager.Instance.ShowCrossHair(true);
            MainUIManager.Instance.GraphicRaycasterEnabled(false);
        }
    }
}