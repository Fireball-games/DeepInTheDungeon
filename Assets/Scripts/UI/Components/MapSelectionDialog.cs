using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.Components
{
    public class MapSelectionDialog : DialogBase
    {
        [SerializeField] private GameObject fileItemPrefab;

        private IEnumerable<Campaign> _campaigns;
        private Campaign _selectedCampaign;
        
        private Transform _campaignsParent;
        private Transform _mapItemsParent;
        private Button _loadLastEditedMapButton;
        private Button _addCampaignButton;
        private Button _addMapButton;
        private Title _selectCampaignTitle;
        private Title _selectMapTitle;
        
        private List<Campaign> _existingCampaigns;
        
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private string[] _existingFiles;
        private string[] _lastLoadedMap;
        
        private bool _showCancelButton;

        private void Awake()
        {
            _existingCampaigns = new List<Campaign>();
            
            Transform content = body.transform.Find("Background/Frame/Content");
            _campaignsParent = content.Find("CampaignsScrollView/Viewport/Content");
            _mapItemsParent = content.Find("MapsScrollView/Viewport/Content");
            _loadLastEditedMapButton = content.Find("LastEditedMapButton").GetComponent<Button>();
            _addCampaignButton = content.Find("AddCampaignButton").GetComponent<Button>();
            _addMapButton = content.Find("AddMapButton").GetComponent<Button>();
            _selectCampaignTitle = content.Find("SelectCampaignTitle").GetComponent<Title>();
            _selectMapTitle = content.Find("SelectMapTitle").GetComponent<Title>();
            
            _loadLastEditedMapButton.onClick.AddListener(LoadLastEditedMap);
            // _addCampaignButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddCampaignKey()));
            // _addMapButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddMapKey()));
        }

        private void LoadLastEditedMap()
        {
            if (_lastLoadedMap is null || _lastLoadedMap.Length != 2)
            {
                Logger.LogError($"Last edited map record in player prefs is null or empty, code should never get here if it is so.");
                return;
            }

            Manager.OrderMapConstruction(_selectedCampaign.GetMapByName(_lastLoadedMap[1]), true);
        }

        public async void Show(bool showCancelButton = true)
        {
            _showCancelButton = showCancelButton;
            
            SetCommonComponents();
            SetExistingCampaigns();
            SetLastEditedMap();
            SetCampaignsScrollView();
            SetMapScrollView();
            SetComponentVisibilities();
            
            await base.Show(t.Get(Keys.MapSelection));
        }

        private void SetLastEditedMap()
        {
            _lastLoadedMap = PlayerPrefs.GetString(Strings.LastEditedMap, null)?.Split('_');

            Campaign campaign = _existingCampaigns.FirstOrDefault(c => c.CampaignName == _lastLoadedMap[0]);
            
            if (campaign is null)
            {
                Logger.LogError($"Last edited map campaign is invalid, campaign name: {_lastLoadedMap[0]}");
                _loadLastEditedMapButton.gameObject.SetActive(false);
                return;
            }
            
            _campaignsParent.Find(campaign.CampaignName).GetComponentInChildren<Button>().SetTextColor(Colors.Positive);
        }

        private void LoadMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return;
            
            Manager.OrderMapConstruction(_selectedCampaign.GetMapByName(mapName), true);
        }

        private void SetExistingCampaigns()
        {
            _existingFiles = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.CampaignDirectoryName);
            _existingCampaigns.Clear();

            if (_existingFiles != null && _existingFiles.Any())
            {
                _existingFiles.ForEach(campaignFile =>
                {
                    try
                    {
                        Campaign loadedCampaign = ES3.Load<Campaign>(Path.GetFileNameWithoutExtension(campaignFile));
                        if (loadedCampaign != null)
                        {
                            _existingCampaigns.Add(loadedCampaign);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Error loading Campaign: {e.Message}");
                    }
                });
            }
        }

        private void SetCampaignsScrollView()
        {
            if (_existingCampaigns == null || !_existingCampaigns.Any())
            {
                _selectedCampaign = null;
                return;
            }
            
            foreach (Button button in _campaignsParent.GetComponentsInChildren<Button>())
            {
                button.SetTextColor(Colors.White);
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject);
            }
            
            _existingCampaigns.ForEach(campaign =>
            {
                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, _campaignsParent.gameObject);
                fileItem.name = campaign.CampaignName;
                fileItem.GetComponentInChildren<TMP_Text>().text = campaign.CampaignName;
                Button button = fileItem.GetComponentInChildren<Button>();
                
                button.onClick.AddListener(() =>
                {
                    button.SetTextColor(Colors.Positive);
                    SelectCampaign(campaign);
                });
            });
        }

        private void SelectCampaign(Campaign campaign)
        {
            _selectedCampaign = campaign;
            SetMapScrollView();
        }

        private void SetCommonComponents()
        {
            title.SetTitle(t.Get(Keys.MapSelection));
            _selectCampaignTitle.SetTitle(t.Get(Keys.SelectCampaignToLoad));
            _selectMapTitle.SetTitle(t.Get(Keys.SelectMapToLoad));
            _loadLastEditedMapButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.LoadLastEditedMap);
            _addCampaignButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.AddCampaign);
            _addMapButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.AddMap);
        }

        private void SetMapScrollView()
        {
            foreach (Button button in _mapItemsParent.GetComponentsInChildren<Button>())
            {
                button.SetTextColor(Colors.White);
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject);
            }

            foreach (MapDescription map in _selectedCampaign.Maps)  
            {
                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, _mapItemsParent.gameObject);
                fileItem.GetComponentInChildren<TMP_Text>().text = map.MapName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => LoadMap(FileOperationsHelper.GetCampaignMapKey(_selectedCampaign.CampaignName, map.MapName)));
            }
        }

        private void SetComponentVisibilities()
        {
            if (_lastLoadedMap is not {Length: 2})
            {
                _loadLastEditedMapButton.gameObject.SetActive(false);
                return;
            }
            
            _loadLastEditedMapButton.gameObject.SetActive(true);
            
            _selectCampaignTitle.gameObject.SetActive(_existingCampaigns != null && _existingCampaigns.Any());
            _addMapButton.gameObject.SetActive(_selectedCampaign != null);
            cancelButton.gameObject.SetActive(_showCancelButton);
        }
    }
}
