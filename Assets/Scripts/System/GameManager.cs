﻿using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private PlayerController playerPrefab;
        private PlayerController player;

        public PlayerController Player => player;
        public Vector3Int PlayerPosition => Player.transform.position.ToVector3Int();
        public MapBuilder MapBuilder => mapBuilder;
        /// <summary>
        /// Current campaign is set only in GameManager and is pointing to either LastEditedCampaign or LastPlayedCampaign from PlayerPrefs. 
        /// </summary>
        public Campaign CurrentCampaign => _currentCampaign;
        public MapDescription CurrentMap => _currentMap;
        public bool MovementEnabled => _movementEnabled;
        public bool IsPlayingFromEditor { get; set; }
        public EGameMode GameMode => _gameMode;

        private Campaign _currentCampaign;
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
            EventsManager.OnSceneStartedLoading += OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading += OnSceneFinishedLoading;

            if (MapBuilder)
            {
                mapBuilder.OnLayoutBuilt.AddListener(OnLayoutBuilt);
            }
        }

        private void OnDisable()
        {
            EventsManager.OnSceneStartedLoading -= OnSceneStartedLoading;
            EventsManager.OnSceneFinishedLoading -= OnSceneFinishedLoading;

            if (MapBuilder)
            {
                mapBuilder.OnLayoutBuilt.RemoveListener(OnLayoutBuilt);
            }
        }
        
        public void ContinueLastPlayedMap()
        {
            // TODO: For now, just starts last played map in last played campaign, once applicable, last position in last played campaign will be loaded. 
            _currentCampaign = FileOperationsHelper.LoadLastPlayedCampaign();
            _currentMap = _currentCampaign.GetStarterMap();
            
            if (_currentCampaign == null || _currentMap == null)
            {
                Logger.LogError("Could not load last played campaign.");
                return;
            }
            
            OnStartGameRequested();
        }

        public void LoadLastEditedMap()
        {
            _currentCampaign = FileOperationsHelper.LoadLastEditedCampaign();
            _currentMap = _currentCampaign.GetMapByName(PlayerPrefsHelper.LastEditedMap[1]);
            
            if (_currentCampaign == null || _currentMap == null)
            {
                Logger.LogError("Could not load last edited map.");
                return;
            }
            
            OnStartGameRequested();
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

        private void OnStartGameRequested()
        {
            SceneLoader.Instance.LoadScene(_currentMap.SceneName);
        }

        private void OnLayoutBuilt()
        {
            if (!_startLevelAfterBuildFinishes) return;
            
            player = ObjectPool.Instance.GetFromPool(playerPrefab.gameObject, Vector3.zero, Quaternion.identity)
                .GetComponent<PlayerController>();
            player.transform.parent = null;
            player.PlayerMovement.SetPositionAndRotation(_currentMap.EditorStartPosition.ToVector3(), CurrentMap.EditorPlayerStartRotation);
            player.PlayerMovement.SetCamera();
            
            _currentCampaign.LastPlayedMap = _currentMap.MapName;
            
            if (!IsPlayingFromEditor)
            {
                FileOperationsHelper.SaveCampaign(_currentCampaign);
            }
            
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

        private async void OnSceneFinishedLoading(string sceneName)
        {
            MapBuilder.DemolishMap();
            
            if (sceneName is Scenes.EditorSceneName)
            {
                _startLevelAfterBuildFinishes = false;
                _gameMode = EGameMode.Editor;
                
                if (IsPlayingFromEditor)
                {
                    MapEditorManager.Instance.OrderMapConstruction(CurrentMap, true);
                }
                else
                {
                    await EditorUIManager.Instance.MapSelectionDialog.Show(false, false);
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
    }
}