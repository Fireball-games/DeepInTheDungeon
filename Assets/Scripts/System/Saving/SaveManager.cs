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
        
        private static MapTraversalData _lastMapExitRecords;

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
        public static void Save(string saveName, bool isAutoSave = false)
        {
            if (!GameManager.Instance.CanSave && !isAutoSave) return;
            
            Instance.StartCoroutine(Instance.SaveCoroutine(saveName));
        }

        private IEnumerator SaveCoroutine(string saveName)
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
            
            save.campaignsSaves = ManageCampaignSaves(_currentSave ?? save).ToList();
            
            _currentSave = save;
            FileOperationsHelper.SavePositionToLocale(save);
        }

        // Checks for current campaign name in GameManager and if its data are already saved in current campaign saves. If yes, copies all mapsSaves from that campaign save and
        // overwrites data for current map state. If not, creates new CampaignSave and adds it to the list with current data.
        private static IEnumerable<CampaignSave> ManageCampaignSaves(Save referenceSave)
        {
            if (referenceSave == null)
            {
                Logger.Log("Reference save is null. aborting saving.");
                return null;
            }
            
            IEnumerable<CampaignSave> campaignSaves = referenceSave.campaignsSaves;
            string currentCampaignName = GameManager.Instance.CurrentCampaign.CampaignName;
            string currentMapName = GameManager.Instance.CurrentMap.MapName;
            
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
        private static IEnumerable<MapStateRecord> CaptureCurrentMapState()
        {
            IEnumerable<ISavable> savables = TriggerService.GetPrefabScripts();
            savables = savables.Concat(TriggerService.TriggerReceivers.Values);

            return savables.Select(savable => new MapStateRecord {guid = savable.Guid, saveData = savable.CaptureState()});
        }

        // Stores map state at the time of traversal trigger activation. These data are supposed to be used when player returns
        // to the map and stored upon player entering the new map into enter map AutoSave.
        public static void StoreTraversalData(Campaign currentCampaign, MapDescription currentMap, EntryPoint currentEntryPoint)
        {
            _lastMapExitRecords = new MapTraversalData
            {
                campaign = currentCampaign,
                map = currentMap,
                entryPoint = currentEntryPoint,
                mapState = CaptureCurrentMapState()
            };
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