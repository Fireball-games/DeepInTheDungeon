using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Building.ItemSpawning;
using Scripts.Building.PrefabsBuilding;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.Player;
using Scripts.ScenesManagement;
using Scripts.System.MonoBases;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Class handling the saving and loading of the game.
    /// </summary>
    public class SaveManager : SingletonNotPersisting<SaveManager>
    {
        // TODO: Make sure, that _currentSave is always up to date and is null when creating new character.
        public static Save CurrentSave { get; set; }
        public static IEnumerable<Save> Saves => _saves;
        
        private static MapBuilder MapBuilder => GameManager.Instance.MapBuilder;
        private static IEnumerable<Save> _saves = Enumerable.Empty<Save>();
        private static Save _tempSave;

        private void OnEnable()
        {
            EventsManager.OnSceneStartedLoading += OnSceneStartedLoading;
            EventsManager.OnNewCampaignStarted.AddListener(OnNewCampaignStarted);
        }

        private void Start()
        {
            UpdateSaves();

            if (!_saves.Any()) return;

            CurrentSave = _saves.First();
        }

        private void OnDisable()
        {
            EventsManager.OnSceneStartedLoading -= OnSceneStartedLoading;
            EventsManager.OnNewCampaignStarted.RemoveListener(OnNewCampaignStarted);
        }

        /// <summary>
        /// Handles saving itself. Assumes, that checking for position with same name, numbering of auto and quick saves etc. is handled at this point.
        /// </summary>
        /// <param name="saveName">Name of saved position stored in save file and name of the save file.</param>
        /// <param name="fileName">Name of the file for saved position, same as saveName if omitted</param>
        /// <param name="isAutoSave">If saved position is AutoSave.</param>
        /// <param name="updatePlayerOnly">If we want to update only player position, like on arrival from previously visited map</param>
        /// <param name="overrideCampaign">Overrides current campaign from GameManager.</param>
        /// <param name="overrideMap">Overrides current map from GameManager</param>
        public static async Task SaveToDisc(string saveName, string fileName, bool isAutoSave = false, bool updatePlayerOnly = false,
            Campaign overrideCampaign = null,
            MapDescription overrideMap = null)
        {
            if (!GameManager.Instance.CanSave && !isAutoSave) return;

            await CreateSave(saveName, fileName, updatePlayerOnly, overrideCampaign, overrideMap);

            SaveToDisc();
        }

        public static async Task SaveToTemp(string saveName, Campaign overrideCampaign, MapDescription overrideMap)
        {
            _tempSave = await CreateSave(saveName, saveName, false, overrideCampaign, overrideMap);
        }

        public static async void QuickSave()
        {
            if (SceneLoader.IsInMainScene || GameManager.Instance.IsPlayingFromEditor) return;

            await SaveToDisc(Keys.QuickSave, Keys.QuickSave, true);
        }
        
        public static MapStateRecord CaptureSavaData(ISavable savable) => new() {guid = savable.Guid, saveData = savable.CaptureState()};
        
        public static void RestoreInventoriesContentFromCurrentSave()
        {
            if (CurrentSave == null) return;

            IEnumerable<ISavable> savables = PlayerController.Instance.InventoryManager.GetInventorySavables();
            RestoreMapData(CurrentSave.InventoriesContent, savables);
        }

        /// <summary>
        /// Creates save file from current save and store that save in _currentSave.
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="fileName"></param>
        /// <param name="updatePlayerOnly"></param>
        /// <param name="overrideCampaign"></param>
        /// <param name="overrideMap"></param>
        /// <returns></returns>
        private static async Task<Save> CreateSave(string saveName, string fileName, bool updatePlayerOnly = false, Campaign overrideCampaign = null,
            MapDescription overrideMap = null)
        {
            byte[] screenshot = await ScreenShotService.Instance.GetCurrentScreenshotBytes();

            fileName = ManagePreviousSavesForName(fileName);

            Save save = new()
            {
                fileName = fileName,
                saveName = saveName,
                // TODO: Add character profile once implemented.
                characterProfile = null,
                playerSaveData = PlayerController.Instance.CaptureState(),
                campaignsSaves = new List<CampaignSave>(),
                timeStamp = DateTime.Now,
                screenshot = screenshot
            };

            if (updatePlayerOnly && CurrentSave != null)
            {
                save.campaignsSaves = CurrentSave.campaignsSaves;
            }
            else
            {
                save.campaignsSaves = CollectCampaignSaves(CurrentSave ?? save, overrideCampaign, overrideMap).ToList();
            }

            // If type of save is MapEntry, copy inventory from current save to new save.
            if (save.saveName == Keys.MapEntry && CurrentSave != null)
            {
                save.playerSaveData.inventoriesContent = CurrentSave.InventoriesContent;
            }
            
            CurrentSave = save;
            return save;
        }

        // Checks for current campaign name in GameManager and if its data are already saved in current campaign saves. If yes, copies all mapsSaves from that campaign save and
        // overwrites data for current map state. If not, creates new CampaignSave and adds it to the list with current data.
        private static IEnumerable<CampaignSave> CollectCampaignSaves(Save referenceSave, Campaign overrideCampaign = null,
            MapDescription overrideMap = null)
        {
            if (referenceSave == null)
            {
                Logger.Log("Reference save is null. aborting saving.");
                return null;
            }

            IEnumerable<CampaignSave> campaignSaves = referenceSave.campaignsSaves;

            string currentCampaignName = overrideCampaign?.CampaignName ?? GameManager.Instance.CurrentCampaign.CampaignName;
            string currentMapName = overrideMap?.MapName ?? GameManager.Instance.CurrentMap.MapName;

            CampaignSave currentCampaignSave = campaignSaves.FirstOrDefault(c => c.campaignName == currentCampaignName);

            if (currentCampaignSave != null)
            {
                MapSave currentMapSave = currentCampaignSave.mapsSaves.FirstOrDefault(m => m.mapName == currentMapName);

                if (currentMapSave != null)
                {
                    currentMapSave.mapState = CaptureCurrentMapSavables();
                    currentMapSave.mapObjects = CaptureCurrentMapObjects();
                }
                else
                {
                    currentCampaignSave.mapsSaves.Add(CaptureCurrentMapState());
                }
            }
            else
            {
                campaignSaves = campaignSaves.Append(new CampaignSave
                {
                    campaignName = currentCampaignName,
                    mapsSaves = new List<MapSave>{ CaptureCurrentMapState() }
                });
            }

            return campaignSaves;
        }

        private static MapSave CaptureCurrentMapState() => new()
        {
            mapName = GameManager.Instance.CurrentMap.MapName,
            mapState = CaptureCurrentMapSavables(),
            mapObjects = CaptureCurrentMapObjects(),
        };

        private void OnNewCampaignStarted()
        {
            CurrentSave = null;
        }

        private static void SaveToDisc()
        {
            Logger.Log(
                $"Saving to disc >  File name: {CurrentSave.fileName.WrapInColor(Colors.Beige)}, Save name: {CurrentSave.saveName.WrapInColor(Colors.Positive)}");
            FileOperationsHelper.SavePositionToLocale(CurrentSave);
            UpdateSaves();
        }

        private static void UpdateSaves()
        {
            FileOperationsHelper.LoadAllSaves(out _saves);

            _saves ??= new List<Save>();

            if (!_saves.Any()) return;

            _saves = _saves.OrderByDescending(s => s.timeStamp);
        }

        private void OnSceneStartedLoading()
        {
            if (_tempSave == null) return;

            CurrentSave = _tempSave;
            SaveToDisc();
            _tempSave = null;
        }

        // Gather all data from all ISavable objects in the scene that need to be saved and stores those data
        // in MapStateRecords.
        private static List<MapStateRecord> CaptureCurrentMapSavables() => GatherSavables().Select(CaptureSavaData).ToList();

        private static List<MapObjectConfiguration> CaptureCurrentMapObjects() => GameManager.Instance.MapBuilder.CollectMapObjects();

        /// <summary>
        /// If player has traversed the map and current save has data about current map, restores map state from the time of traversal trigger activation.
        /// </summary>
        public static async Task RestoreMapDataFromCurrentSave(string mapName)
        {
            if (CurrentSave == null)
            {
                Logger.Log("Current save is null. Aborting restoring map data.");
                return;
            }

            await RestoreMapDataFromSave(CurrentSave, mapName);
        }

        private static async Task RestoreMapDataFromSave(Save save, string mapName)
        {
            CampaignSave campaignSave = save.campaignsSaves.FirstOrDefault(c => c.campaignName == save.CurrentCampaign);
            if (campaignSave == null)
            {
                Logger.Log("Current campaign save is null. Aborting restoring map data.");
                return;
            }

            MapSave currentMapSave = campaignSave.mapsSaves.FirstOrDefault(m => m.mapName == mapName);
            if (currentMapSave == null)
            {
                Logger.Log("Current map save is null. Aborting restoring map data.");
                return;
            }

            RestoreMapData(currentMapSave.mapState);

            if (currentMapSave.mapObjects != null)
            {
                GameManager.Instance.CurrentMap.MapObjects = currentMapSave.mapObjects;
                await MapBuilder.RebuildItems();
            }
        }

        private static void RestoreMapData(List<MapStateRecord> mapStates, IEnumerable<ISavable> savables)
        {
            foreach (ISavable savable in savables)
            {
                MapStateRecord record = mapStates.FirstOrDefault(r => r.guid == savable.Guid);
                if (record != null)
                {
                    savable.RestoreState(record.saveData);
                }
            }
        }
        
        private static void RestoreMapData(List<MapStateRecord> mapStates) =>
            RestoreMapData(mapStates, GatherSavables().Concat(PlayerController.Instance.InventoryManager.GetInventorySavables()));

        private static string ManagePreviousSavesForName(string rootFileName)
        {
            IEnumerable<string> currentNames = FileOperationsHelper.GetFileNamesAndRemoveOldestSaveForName(rootFileName);
            UpdateSaves();

            return currentNames == null || !currentNames.Any()
                ? $"{rootFileName}1"
                : rootFileName.IncrementName(currentNames);
        }

        private static IEnumerable<ISavable> GatherSavables()
        {
            IEnumerable<ISavable> savables = TriggerService.GetPrefabScripts().ToList();
            savables = savables.Concat(TriggerService.TriggerReceivers.Values);

            return savables;
        }
    }
}