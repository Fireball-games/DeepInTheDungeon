using System;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.System.Saving;
using Scripts.Triggers;
using Scripts.UI;
using UnityEngine;
using static Scripts.UI.MainUIManager;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System
{
    public class MapTraversal
    {
        public Campaign CurrentCampaign => _currentCampaign;
        public MapDescription CurrentMap => _currentMap;
        internal bool EntryMovementFinished => _entryMovementFinished;

        private static GameManager GameManager => GameManager.Instance;
        private static MapBuilder MapBuilder => GameManager.MapBuilder;
        private static MainUIManager UIManager => SingletonNotPersisting<MainUIManager>.Instance;
        private PlayerCameraController PlayerCamera => PlayerCameraController.Instance;

        private PlayerController Player
        {
            get => GameManager.Player;
            set => GameManager.player = value;
        }

        private bool MovementEnabled
        {
            set => GameManager.movementEnabled = value;
        }

        private readonly Campaign _mainCampaign;
        private readonly Campaign _startRoomsCampaign;
        private readonly PlayerController _playerPrefab;

        private Campaign _currentCampaign;
        private MapDescription _currentMap;
        private EntryPoint _currentEntryPoint;

        private bool _lookModeOnStartTraversal;
        private bool _entryMovementFinished;

        private Action _onMovementFinished;

        public MapTraversal(PlayerController playerPrefab)
        {
            _playerPrefab = playerPrefab;

            if (!FileOperationsHelper.LoadSystemCampaigns(out _mainCampaign, out _startRoomsCampaign))
            {
                Logger.LogError("Failed to load system campaigns.");
            }

            _currentEntryPoint = new EntryPoint();
        }

        internal bool SetForStartFromMainScreen()
        {
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
                    playerRotationY = (int) _currentMap.EditorPlayerStartRotation.eulerAngles.y
                };
            }

            if (_currentCampaign != null && _currentMap != null) return true;

            Logger.LogError("Could not load last played campaign.");
            return false;
        }

        internal bool SetForStartingMainCampaign()
        {
            _currentCampaign = _mainCampaign;
            _currentMap = _currentCampaign.GetStarterMap();
            _currentEntryPoint = _currentMap.EntryPoints[0].Cloned();

            StartMapFromMainScreenButtonClickHandling();

            return true;
        }

        public bool SetForStartingFromSave()
        {
            if (!SetCampaignFromLastSave()) return false;

            StartMapFromMainScreenButtonClickHandling();

            return true;
        }
        
        public bool SetForQuickLoad()
        {
            return SetCampaignFromLastSave();
        }

        public bool SetForStartingFromLastEditedMap(EntryPoint entryPoint)
        {
            if (!FileOperationsHelper.GetLastEditedCampaignAndMap(out _currentCampaign, out _currentMap))
            {
                return false;
            }

            if (entryPoint == null)
            {
                _currentEntryPoint.playerGridPosition = _currentMap.EditorStartPosition;
                _currentEntryPoint.playerRotationY = (int) _currentMap.EditorPlayerStartRotation.eulerAngles.y;
                _currentEntryPoint.isMovingForwardOnStart = false;
            }
            else
            {
                _currentEntryPoint = entryPoint.Cloned();
            }

            return true;
        }

        public bool SetForTraversal(string exitConfigurationGuid, out float exitDelay)
        {
            exitDelay = 0f;

            if (CurrentMap == null || !_entryMovementFinished) return false;

            if (!GameManager.IsPlayingFromEditor)
            {
                SaveManager.SaveToTemp(Keys.MapExit, CurrentCampaign, CurrentMap);
            }

            TriggerConfiguration mapTraversal = TriggerService.GetConfiguration(exitConfigurationGuid);

            if (mapTraversal == null)
            {
                Logger.LogWarning($"Could not find exit point trigger configuration with guid {exitConfigurationGuid}.");
                return false;
            }

            // Storing so if getting the data fails, game can continue.
            MapDescription mapDescription = _currentCampaign.GetMapByName(mapTraversal.TargetMapName);

            if (mapDescription == null)
            {
                Logger.LogWarning($"Could not find map with name {mapTraversal.TargetMapName}.");
                return false;
            }

            EntryPoint entryPoint = mapDescription.GetEntryPointCloneByName(mapTraversal.TargetMapEntranceName);

            if (entryPoint == null)
            {
                Logger.LogWarning(
                    $"Could not find entry point in map: {mapTraversal.TargetMapName.WrapInColor(Colors.Warning)} with name: {mapTraversal.TargetMapEntranceName.WrapInColor(Colors.Warning)}.");
                return false;
            }

            _currentMap = mapDescription;
            _currentEntryPoint = entryPoint;

            _lookModeOnStartTraversal = PlayerCamera.IsLookModeOn;
            PlayerCamera.IsLookModeOn = false;

            exitDelay = mapTraversal.ExitDelay;
            return true;
        }

        public async Task OnLayoutBuilt()
        {
            PlayerCamera.IsLookModeOn = false;
            MouseCursorManager.ResetCursor();
            
            Player = ObjectPool.Instance.SpawnFromPool(_playerPrefab.gameObject, Vector3.zero, Quaternion.identity)
                .GetComponent<PlayerController>();
            Player.transform.parent = null;
            Player.PlayerMovement.SetPositionAndRotation(
                _currentEntryPoint.playerGridPosition,
                Quaternion.Euler(0f, _currentEntryPoint.playerRotationY, 0f));
            Player.PlayerMovement.SetCamera();

            // To allow playing StartRooms from Editor
            MovementEnabled = true;

            if (!SceneLoader.IsInMainScene && !GameManager.IsPlayingFromEditor)
            {
                SaveManager.RestoreMapDataFromCurrentSave();
            }
            
            ScreenFader.FadeOut(1.2f);

            await Task.Delay(200);


            if (_currentEntryPoint.isMovingForwardOnStart)
            {
                MovementEnabled = false;
                _entryMovementFinished = false;

                if (SceneLoader.IsInMainScene && !GameManager.IsPlayingFromEditor)
                {
                    HandleEntryMovement(SetControlsForMainScene);
                }
                else
                {
                    HandleEntryMovement(HandleFirstStepAfterTraversal);
                }

                Player.PlayerMovement.MoveForward(true);
                return;
            }
            
            Player.PlayerMovement.SetDefaultTransitionSpeed();
        }

        public void SetCurrentCampaign(Campaign newCampaign)
        {
            _currentCampaign = newCampaign;
        }

        public void SetCurrentMap(MapDescription mapDescription)
        {
            if (mapDescription == null) return;

            _currentMap = mapDescription;

            MapBuilder.SetLayout(mapDescription.Layout);
        }

        public void CheckCurrentMap()
        {
            _currentMap ??= CurrentCampaign.GetStarterMap();
        }

        private bool SetCampaignFromLastSave()
        {
            Save lastSave = SaveManager.CurrentSave;

            if (lastSave == null)
            {
                Logger.LogWarning("Could not load last save.");
                return false;
            }

            if (lastSave.playerSaveData.currentCampaign == Strings.MainCampaignName)
            {
                _currentCampaign = _mainCampaign;
            }
            else
            {
                FileOperationsHelper.LoadCampaign(lastSave.CurrentCampaign, out _currentCampaign);
            }

            if (_currentCampaign == null)
            {
                Logger.LogError("Could not load last played campaign.");
                return false;
            }

            _currentMap = _currentCampaign.GetMapByName(lastSave.CurrentMap);

            if (_currentMap == null)
            {
                Logger.LogError("Failed to get last played map.");
                return false;
            }

            _currentEntryPoint.playerGridPosition = lastSave.PlayerGridPosition;
            _currentEntryPoint.playerRotationY = (int) lastSave.PlayerRotation.eulerAngles.y;
            _currentEntryPoint.isMovingForwardOnStart = false;

            return true;
        }

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
            GameManager.CanSave = true;
            PlayerMovement.OnStartResting.RemoveListener(OnMovementFinishedWrapper);
        }
        
        private async void StartMapFromMainScreenButtonClickHandling()
        {
            PlayerCameraController.Instance.IsLookModeOn = false;
            UIManager.ShowCrossHairAsync(false);
            await UIManager.ShowMainMenu(false);

            GameManager.Player.PlayerMovement.MoveForward(true);
            GameObject.FindObjectOfType<DoTweenTriggerReceiver>().Trigger();
        }

        private async void HandleFirstStepAfterTraversal()
        {
            if (!GameManager.IsPlayingFromEditor)
            {
                await SaveManager.SaveToDisc(Keys.MapEntry, true, true);
            }

            // To prevent moving before some async operations finishes
            await Task.Delay(100);
            MovementEnabled = true;
            PlayerCamera.IsLookModeOn = _lookModeOnStartTraversal;
        }

        private async void SetControlsForMainScene()
        {
            MovementEnabled = false;
            PlayerCameraController.Instance.IsLookModeOn = true;
            PlayerCameraController.Instance.SetRotationLimits(new RotationSettings
            {
                MinXRotation = -60f,
                MaxXRotation = 60f,
                MinYRotation = -85f,
                MaxYRotation = 85f
            });

            UIManager.ShowCrossHairAsync(true);
            UIManager.GraphicRaycasterEnabled(false);
            await UIManager.ShowMainMenu(true, ETargetedMainMenu.OnWorldCanvas);
        }
    }
}