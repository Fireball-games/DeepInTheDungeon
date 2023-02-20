using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building;
using Scripts.Building.PrefabsBuilding;
using Scripts.Helpers;
using Scripts.Player;
using Scripts.System.MonoBases;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Class handling the saving and loading of the game.
    /// </summary>
    public class SaveManager : SingletonNotPersisting<SaveManager>
    {
        private static IEnumerable<Save> _saves = Enumerable.Empty<Save>();
        // TODO: Make sure, that _currentSave is always up to date and is null when creating new character.
        private static Save _currentSave;
        
        private void Start()
        {
            if (!_saves.Any() && (FileOperationsHelper.LoadAllSaves(out IEnumerable<Save> saves)))
            {
                _saves = saves;
            }
        }

        /// <summary>
        /// Handles saving itself. Assumes, that checking for position with same name, numbering of auto and quick saves etc. is handled at this point.
        /// </summary>
        /// <param name="saveName">Name of saved position stored in save file and name of the save file.</param>
        /// <param name="isAutoSave">If saved position is AutoSave.</param>
        /// <param name="updatePlayerOnly">If we want to update only player position, like on arrival from previously visited map</param>
        /// <param name="overrideCampaign">Overrides current campaign from GameManager.</param>
        /// <param name="overrideMap">Overrides current map from GameManager</param>
        public static void Save(string saveName, bool isAutoSave = false, bool updatePlayerOnly = false, Campaign overrideCampaign = null, MapDescription overrideMap = null)
        {
            if (!GameManager.Instance.CanSave && !isAutoSave) return;
            
            Instance.StartCoroutine(Instance.SaveCoroutine(saveName, updatePlayerOnly, overrideCampaign, overrideMap));
        }

        private IEnumerator SaveCoroutine(string saveName, bool updatePlayerOnly = false, Campaign overrideCampaign = null, MapDescription overrideMap = null)
        {
            yield return new WaitForEndOfFrame();
            
            byte[] screenshot = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();

            Save save = new()
            {
                saveName = saveName,
                // TODO: Add character profile once implemented.
                characterProfile = null,
                playerSaveData = PlayerController.Instance.CaptureState(),
                campaignsSaves = new List<CampaignSave>(),
                timeSaved = DateTime.Now,
                screenshot = screenshot
            };
            
            if (updatePlayerOnly && _currentSave != null)
            {
                save.campaignsSaves = _currentSave.campaignsSaves;
            }
            else
            {
                save.campaignsSaves = ManageCampaignSaves(_currentSave ?? save, overrideCampaign, overrideMap).ToList();
            }
            Logger.Log($"Saving to {saveName}");
            _currentSave = save;
            FileOperationsHelper.SavePositionToLocale(save);
        }

        // Checks for current campaign name in GameManager and if its data are already saved in current campaign saves. If yes, copies all mapsSaves from that campaign save and
        // overwrites data for current map state. If not, creates new CampaignSave and adds it to the list with current data.
        private static IEnumerable<CampaignSave> ManageCampaignSaves(Save referenceSave, Campaign overrideCampaign = null,
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
                    currentMapSave.mapState = CaptureCurrentMapState();
                }
                else
                {
                    currentCampaignSave.mapsSaves.Add(new MapSave
                    {
                        mapName = currentMapName,
                        mapState = CaptureCurrentMapState()
                    });
                }
            }
            else
            {
                campaignSaves = campaignSaves.Append(new CampaignSave
                {
                    campaignName = currentCampaignName,
                    mapsSaves = new List<MapSave>
                    {
                        new()
                        {
                            mapName = currentMapName,
                            mapState = CaptureCurrentMapState()
                        }
                    }
                });
            }

            return campaignSaves;
        }

        // Gather all data from all ISavable objects in the scene that need to be saved and stores those data
        // in MapStateRecords.
        private static List<MapStateRecord> CaptureCurrentMapState()
        {
            IEnumerable<ISavable> savables = TriggerService.GetPrefabScripts().ToList();
            savables = savables.Concat(TriggerService.TriggerReceivers.Values);
            List<MapStateRecord> result = savables.Select(CaptureSavaData).ToList();
            return result;
        }

        private static MapStateRecord CaptureSavaData(ISavable savable)
        {
            return new MapStateRecord {guid = savable.Guid, saveData = savable.CaptureState()};
        }

        /// <summary>
        /// If player has traversed the map and current save has data about current map, restores map state from the time of traversal trigger activation.
        /// </summary>
        public static void RestoreMapDataFromCurrentSave()
        {
            if (_currentSave == null)
            {
                Logger.Log("Current save is null. Aborting restoring map data.");
                return;
            }
            
            CampaignSave currentCampaignSave = _currentSave.campaignsSaves.FirstOrDefault(c => c.campaignName == GameManager.Instance.CurrentCampaign.CampaignName);
            if (currentCampaignSave == null)
            {
                Logger.Log("Current campaign save is null. Aborting restoring map data.");
                return;
            }
            
            MapSave currentMapSave = currentCampaignSave.mapsSaves.FirstOrDefault(m => m.mapName == GameManager.Instance.CurrentMap.MapName);
            if (currentMapSave == null)
            {
                Logger.Log("Current map save is null. Aborting restoring map data.");
                return;
            }
            
            RestoreMapData(currentMapSave.mapState);
        }

        private static void RestoreMapData(List<MapStateRecord> mapState)
        {
            Logger.Log("Restoring map data.");
            IEnumerable<ISavable> savables = TriggerService.GetPrefabScripts().ToList();
            savables = savables.Concat(TriggerService.TriggerReceivers.Values);
            
            foreach (ISavable savable in savables)
            {
                MapStateRecord record = mapState.FirstOrDefault(r => r.guid == savable.Guid);
                if (record != null)
                {
                    savable.RestoreState(record.saveData);
                }
            }
        }
    }
    
    internal class MapTraversalData
    {
        public Campaign campaign;
        public MapDescription map;
        public EntryPoint entryPoint;
        public IEnumerable<MapStateRecord> mapState;
    }
}