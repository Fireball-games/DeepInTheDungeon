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
        
        private Button _loadLastEditedMapButton;
        
        private Title _selectCampaignTitle;
        private GameObject _campaignsScrollView;
        private Transform _campaignsParent;
        private Button _addCampaignButton;
        
        private Title _selectMapTitle;
        private GameObject _mapsScrollView;
        private Transform _mapsItemsParent;
        private Button _addMapButton;
        
        private List<Campaign> _existingCampaigns;
        
        private static MapEditorManager Manager => MapEditorManager.Instance;

        private string[] _existingFiles;
        private string[] _lastEditedMap;
        
        private bool _showCancelButton;

        private void Awake()
        {
            _existingCampaigns = new List<Campaign>();
            
            Transform content = body.transform.Find("Background/Frame/Content");
            
            _loadLastEditedMapButton = content.Find("LastEditedMapButton").GetComponent<Button>();
            
            _campaignsParent = content.Find("CampaignsScrollView/Viewport/Content");
            _campaignsScrollView = content.Find("CampaignsScrollView").gameObject;
            _selectCampaignTitle = content.Find("SelectCampaignTitle").GetComponent<Title>();
            _addCampaignButton = content.Find("AddCampaignButton").GetComponent<Button>();
            
            _selectMapTitle = content.Find("SelectMapTitle").GetComponent<Title>();
            _mapsScrollView = content.Find("MapsScrollView").gameObject;
            _mapsItemsParent = content.Find("MapsScrollView/Viewport/Content");
            _addMapButton = content.Find("AddMapButton").GetComponent<Button>();
            
            _loadLastEditedMapButton.onClick.AddListener(LoadLastEditedMap);
            // _addCampaignButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddCampaignKey()));
            // _addMapButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddMapKey()));
        }

        private void LoadLastEditedMap()
        {
            if (_lastEditedMap is null || _lastEditedMap.Length != 2)
            {
                Logger.LogError($"Last edited map record in player prefs is null or empty, code should never get here if it is so.");
                return;
            }

            Manager.OrderMapConstruction(_selectedCampaign.GetMapByName(_lastEditedMap[1]), true);
        }

        public async void Show(bool showCancelButton = true)
        {
            _showCancelButton = showCancelButton;
            
            SetCommonComponents();
            SetExistingCampaigns();
            SetCampaignsScrollView();
            SetMapScrollView();
            SetLastEditedMap();
            SetComponentVisibilities();
            
            await base.Show(t.Get(Keys.MapSelection));
        }

        private void SetLastEditedMap()
        {
            _lastEditedMap = PlayerPrefs.GetString(Strings.LastEditedMap, null)?.Split('_');

            if (_lastEditedMap == null) return;
            
            if (string.IsNullOrEmpty(_lastEditedMap[0]))
            {
                Logger.LogError($"Last edited map campaign is null or empty, code should never get here if it is so.");
                return;
            }
            
            OnCampaignSelected(_existingCampaigns.FirstOrDefault(c => c.CampaignName == _lastEditedMap[0]));

            if (_selectedCampaign is not null) return;
            
            Logger.LogError($"Last edited map campaign is invalid, campaign name: {_lastEditedMap[0]}");
            _loadLastEditedMapButton.gameObject.SetActive(false);
        }

        private void OnCampaignSelected(Campaign selectedCampaign)
        {
            _selectedCampaign = selectedCampaign;
            SetMapScrollView();
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
                    SetScrollViewButtonSelected(campaign.CampaignName, _campaignsParent);
                    OnCampaignSelected(campaign);
                });
            });
        }

        private void SetScrollViewButtonSelected(string selectedItemName, Transform scrollViewParent)
        {
            foreach (Button button in scrollViewParent.GetComponentsInChildren<Button>())
            {
                button.SetTextColor(button.name == selectedItemName ? Colors.Positive : Colors.White);
            }
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
            foreach (Button button in _mapsItemsParent.GetComponentsInChildren<Button>())
            {
                button.SetTextColor(Colors.White);
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject);
            }

            foreach (MapDescription map in _selectedCampaign.Maps)  
            {
                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, _mapsItemsParent.gameObject);
                fileItem.GetComponentInChildren<TMP_Text>().text = map.MapName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => LoadMap(FileOperationsHelper.GetCampaignMapKey(_selectedCampaign.CampaignName, map.MapName)));
            }
        }

        private void SetComponentVisibilities()
        {
            if (_lastEditedMap is not {Length: 2})
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
