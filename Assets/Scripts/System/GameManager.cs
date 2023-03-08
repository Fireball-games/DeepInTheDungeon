using System.Threading.Tasks;
using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.Player;
using Scripts.Player.CharacterSystem;
using Scripts.ScenesManagement;
using Scripts.ScriptableObjects;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
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
        
        private MapTraversal _mapTraversal;

        public GameConfiguration GameConfiguration => gameConfiguration;
        public PlayerController Player => player;
        public Vector3Int PlayerPosition => Player.transform.position.ToVector3Int();
        public MapBuilder MapBuilder => mapBuilder;
        /// <summary>
        /// Current campaign is set only in GameManager and is pointing to LastEditedCampaign, StartRooms, SelectedCampaign or Loaded campaign. 
        /// </summary>
        public Campaign CurrentCampaign => _mapTraversal.CurrentCampaign;
        public MapDescription CurrentMap => _mapTraversal.CurrentMap;
        public bool MovementEnabled => movementEnabled;
        public bool IsPlayingFromEditor { get; set; }
        public EGameMode GameMode => _gameMode;
        public bool CanSave { get; internal set; }
        public CharacterProfile CurrentCharacterProfile { get; private set; }

        internal PlayerController player;
        internal bool movementEnabled;

        private EGameMode _gameMode = EGameMode.Play;

        private bool _startLevelAfterBuildFinishes;
        private bool _isPlayingFromSavedGame;

        public enum EGameMode
        {
            MainScene = 0,
            Play = 1,
            Editor = 2,
        }

        protected override void Awake()
        {
            base.Awake();
            
            _mapTraversal = new MapTraversal(playerPrefab);
            CurrentCharacterProfile = new CharacterProfile();
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

        public void SetCurrentCampaign(Campaign campaign) => _mapTraversal.SetCurrentCampaign(campaign);

        public void SetCurrentMap(MapDescription mapDescription)
        {
            _mapTraversal.SetCurrentMap(mapDescription);
        }
        
        public void StartMainScene(bool fadeIn = true)
        {
            if(!_mapTraversal.SetForStartFromMainScreen())
            {
                Logger.LogWarning("Could not start main scene.");
                return;
            }
            
            OnStartGameRequested(fadeIn);
        }

        public async void StartMainCampaign()
        {
            // TODO: When applicable, handle warning about deleting save files.

            if (!_mapTraversal.SetForStartingMainCampaign())
            {
                Logger.LogWarning("Could not start new campaign.");
                return;
            }
            EventsManager.TriggerOnNewCampaignStarted();
            await Task.Delay(2500);
            
            OnStartGameRequested();
        }
        
        public void ContinueFromSave(Save save)
        {
            if (!_mapTraversal.SetForStartingFromSave(save))
            {
                Logger.LogWarning("Could not continue from save.");
                return;
            }
            
            OnStartGameRequested();
        }
        
        public void QuickLoad()
        {
            if (IsPlayingFromEditor) return;
            
            if (!_mapTraversal.SetCampaignFromSave(SaveManager.CurrentSave))
            {
                Logger.LogWarning("Could not quick load.");
                return;
            }
            
            OnStartGameRequested();
        }

        public void LoadSavedPosition(Save save)
        {
            if (!_mapTraversal.SetCampaignFromSave(save))
            {
                Logger.LogWarning("Failed to load saved position.");
                return;
            }
            
            OnStartGameRequested();
        }

        public void LoadLastEditedMap(EntryPoint entryPoint = null)
        {
            if (!_mapTraversal.SetForStartingFromLastEditedMap(entryPoint))
            {
                Logger.LogWarning("Could not load last edited map.");
                return;
            }
            
            OnStartGameRequested();
        }
        
        public void OnLocalizationChanged()
        {
            Logger.LogNotImplemented();
        }

        private async void OnMapTraversalTriggered(string exitConfigurationGuid)
        {
            float? exitDelay = await _mapTraversal.SetForTraversal(exitConfigurationGuid);
            
            if (exitDelay == null)
            {
                if (_mapTraversal.EntryMovementFinished) Logger.LogWarning("Could not traverse map.");
                return;
            }

            CanSave = false;
            
            await Task.Delay((int)exitDelay * 1000);
            
            OnStartGameRequested();
        }
        

        private void OnStartGameRequested(bool fadeIn = true)
        {
            CanSave = false;
            SceneLoader.Instance.LoadScene(CurrentMap.SceneName, fadeIn, 1f);
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
                if (!IsPlayingFromEditor)
                {
                    _gameMode = EGameMode.MainScene;
                }
            }

            StartBuildingLevel();
        }
        
        private void StartBuildingLevel()
        {
            if (CurrentCampaign == null)
            {
                Logger.LogWarning("No Campaign is set, this should not happen here, ever.");
                return;
            }
            
            _gameMode = EGameMode.Play;
            
            movementEnabled = false;

            _startLevelAfterBuildFinishes = true;

            _mapTraversal.CheckCurrentMap();

            PlayerPrefs.SetString(Strings.LastPlayedCampaign,CurrentCampaign.CampaignName);
            
            mapBuilder.BuildMap(CurrentMap);
        }

        private async void OnLayoutBuilt()
        {
            if (!_startLevelAfterBuildFinishes) return;
            
            await _mapTraversal.OnLayoutBuilt();
            
            EventsManager.TriggerOnLevelStarted();
        }
    }
}