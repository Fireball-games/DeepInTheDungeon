using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scripts.Helpers.Strings;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.UI.Components
{
    public class MapSelectionDialog : DialogBase
    {
        [SerializeField] private GameObject fileItemPrefab;

        private IEnumerable<Campaign> _campaigns;
        private Campaign _selectedCampaign;
        
        private Button _loadLastEditedMapButton;
        private Title _lastEditedMapDescription;
        
        private Title _selectCampaignTitle;
        private GameObject _campaignsScrollView;
        private Transform _campaignsParent;
        private Button _addCampaignButton;
        
        private Title _selectCampaignPrompt;
        private Title _selectMapPrompt;
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
            _lastEditedMapDescription = content.Find("LastEditedMapDescription").GetComponent<Title>();
            
            _campaignsParent = content.Find("CampaignsScrollView/Viewport/Content");
            _campaignsScrollView = content.Find("CampaignsScrollView").gameObject;
            _selectCampaignTitle = content.Find("SelectCampaignTitle").GetComponent<Title>();
            _addCampaignButton = content.Find("AddCampaignButton").GetComponent<Button>();
            
            _selectCampaignPrompt = content.Find("SelectCampaignPrompt").GetComponent<Title>();
            _selectMapPrompt = content.Find("SelectMapPrompt").GetComponent<Title>();
            _selectMapTitle = content.Find("SelectMapTitle").GetComponent<Title>();
            _mapsScrollView = content.Find("MapsScrollView").gameObject;
            _mapsItemsParent = content.Find("MapsScrollView/Viewport/Content");
            _addMapButton = content.Find("AddMapButton").GetComponent<Button>();
            
            _loadLastEditedMapButton.onClick.AddListener(LoadLastEditedMap);
            _addCampaignButton.onClick.AddListener(AddCampaign);
            _addMapButton.onClick.AddListener(AddMap);
        }

        public async Task Show(bool showCancelButton = true, bool isModalClosingDialog = true)
        {
            _showCancelButton = showCancelButton;
            
            SetCommonComponents();
            SetExistingCampaigns();
            SetCampaignsScrollView();
            SetMapScrollView();
            SetLastEditedMap();
            RedrawComponents();
            
            await base.Show(t.Get(Keys.MapSelection), isModalClosingDialog: isModalClosingDialog);
        }

        private async void AddCampaign()
        {
            string defaultCampaignName = GetDefaultName(
                t.Get(Keys.Campaign),
                _existingCampaigns.Select(c => c.CampaignName));
            string campaignName = await EditorUIManager.Instance.ShowInputFieldDialog(t.Get(Keys.EnterCampaignName), defaultCampaignName);
            
            if (string.IsNullOrEmpty(campaignName)) return;
            
            Campaign campaign = new() {CampaignName = campaignName};
            
            _existingCampaigns.Add(campaign);
            
            ES3.Save(campaignName, campaign, FileOperationsHelper.GetSavePath(campaignName));
            
            _selectedCampaign = campaign;
            GameManager.Instance.SetCurrentCampaign(campaign);
            SetCampaignsScrollView();
            SetScrollViewButtonSelected(campaignName, _campaignsParent);
            RedrawComponents();
        }
        
        private async void AddMap()
        {
            if (_selectedCampaign == null)
            {
                Logger.LogWarning("No map selected, can't add map.");
                return;
            }
            
            NewMapDialog dialog = EditorUIManager.Instance.NewMapDialog;
            
            if (await dialog.Show(_selectedCampaign, false) is EConfirmResult.Cancel) return;
            
            int rows = int.Parse(dialog.rowsInput.Text);
            int columns = int.Parse(dialog.columnsInput.Text);
            int floors = int.Parse(dialog.floorsInput.Text) + 2;
            string mapName = dialog.mapNameInput.Text;

            MapDescription newMap = MapBuilder.GenerateDefaultMap(
                Mathf.Clamp(floors, MapEditorManager.MinFloors, MapEditorManager.MaxFloors),
                Mathf.Clamp(rows, MapEditorManager.MinRows, MapEditorManager.MaxRows),
                Mathf.Clamp(columns, MapEditorManager.MinColumns, MapEditorManager.MaxColumns));

            newMap.MapName = string.IsNullOrEmpty(mapName)
                ? GetDefaultName(
                    t.Get(Keys.NewMapName),
                    _selectedCampaign.Maps.Select(m => m.MapName)
                )
                : mapName;
            
            if (_selectedCampaign.Maps.Any(m => m.MapName == newMap.MapName))
            {
                string message = $"{mapName}: {t.Get(Keys.MapAlreadyExists)}";
                EditorUIManager.Instance.MessageBar.Set(message, MessageBar.EMessageType.Warning, automaticDismissDelay: 3f);
                return;
            }
            
            _selectedCampaign.AddReplaceMap(newMap);
            GameManager.Instance.SetCurrentMap(newMap);
            Manager.SaveMap();
            SetMapScrollView();
            RedrawComponents();

            CloseDialog();
            Manager.OrderMapConstruction(newMap, true);
        }

        private void LoadLastEditedMap()
        {
            if (_lastEditedMap is null || _lastEditedMap.Length != 2)
            {
                Logger.LogError($"Last edited map record in player prefs is null or empty, code should never get here if it is so.");
                return;
            }
            
            CloseDialog();
            Manager.OrderMapConstruction(_selectedCampaign.GetMapByName(_lastEditedMap[1]), true);
        }

        private void SetLastEditedMap()
        {
            _lastEditedMap = PlayerPrefs.GetString(LastEditedMap, null)?.Split('_');

            if (!IsLastEditedMapValid()) return;

            OnCampaignSelected(_existingCampaigns.FirstOrDefault(c => c.CampaignName == _lastEditedMap[0]));

            if (_selectedCampaign is not null)
            {
                SetScrollViewButtonSelected(_lastEditedMap[1], _mapsItemsParent);
                return;
            }
            
            Logger.LogError($"Last edited map campaign is invalid, campaign name: {_lastEditedMap[0]}");
            _loadLastEditedMapButton.gameObject.SetActive(false);
        }

        private void OnCampaignSelected(Campaign selectedCampaign)
        {
            _selectedCampaign = selectedCampaign;
            GameManager.Instance.SetCurrentCampaign(_selectedCampaign);
            SetScrollViewButtonSelected(selectedCampaign.CampaignName, _campaignsParent);
            SetMapScrollView();
            RedrawComponents();
        }

        private void LoadMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return;
            
            CloseDialog();
            Manager.OrderMapConstruction(_selectedCampaign.GetMapByName(mapName), true);
        }

        private void SetExistingCampaigns()
        {
            // no need to create new list if its not empty
            if (_existingCampaigns.Any()) return;
            
            _existingFiles = FileOperationsHelper.GetFilesInDirectory(FileOperationsHelper.CampaignDirectoryName);
            _existingCampaigns.Clear();

            if (_existingFiles != null && _existingFiles.Any())
            {
                _existingFiles.ForEach(campaignFile =>
                {
                    try
                    {
                        string campaignName = Path.GetFileNameWithoutExtension(campaignFile);
                        Campaign loadedCampaign = ES3.Load<Campaign>(campaignName, FileOperationsHelper.GetSavePath(campaignName));
                        if (loadedCampaign != null)
                        {
                            _existingCampaigns.Add(loadedCampaign);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Error loading Campaign: {e.Message}");
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
                button.SetTextColor(button.GetComponentInChildren<TMP_Text>().text == selectedItemName ? Colors.Positive : Colors.White);
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
            if (_selectedCampaign == null) return;

            foreach (Button button in _mapsItemsParent.GetComponentsInChildren<Button>())
            {
                button.SetTextColor(Colors.White);
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject);
            }

            foreach (MapDescription map in _selectedCampaign.Maps)  
            {
                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, _mapsItemsParent.gameObject);
                fileItem.name = map.MapName;
                fileItem.GetComponentInChildren<TMP_Text>().text = map.MapName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => LoadMap(map.MapName));
            }
        }
        
        /// <summary>
        /// Resolve if _lastEditedMap has valid values.
        /// </summary>
        private bool IsLastEditedMapValid()
        {
            if (_lastEditedMap is not {Length: 2}) return false;
            
            if (string.IsNullOrEmpty(_lastEditedMap[0])) return false;
            
            if (string.IsNullOrEmpty(_lastEditedMap[1])) return false;
            
            return true;
        }

        private void RedrawComponents()
        {
            bool lastEditedMapValid = IsLastEditedMapValid();
            _loadLastEditedMapButton.gameObject.SetActive(lastEditedMapValid);
            
            if (lastEditedMapValid)
            {
                _lastEditedMapDescription.SetTitle($"{t.Get(Keys.Campaign)}: {_lastEditedMap[0]} {t.Get(Keys.Map)}: {_lastEditedMap[1]}");
                _lastEditedMapDescription.SetCollapsed(false);
            }
            else
            {
                _lastEditedMapDescription.SetCollapsed(true);
            }
            
            _selectCampaignTitle.gameObject.SetActive(_existingCampaigns.Any());
            _campaignsScrollView.gameObject.SetActive(_existingCampaigns.Any());
            
            bool isCampaignSelected = _selectedCampaign != null;

            _selectCampaignPrompt.Show(!isCampaignSelected ? t.Get(Keys.SelectCampaignPrompt) : null);
            
            _selectMapPrompt.Show(isCampaignSelected && !_selectedCampaign.Maps.Any() ? t.Get(Keys.SelectMapPrompt) : null);
            
            bool isMapsViewPresentable = isCampaignSelected && _selectedCampaign.Maps.Any();
            
            _selectMapTitle.gameObject.SetActive(isMapsViewPresentable);
            _mapsScrollView.gameObject.SetActive(isMapsViewPresentable);
            _addMapButton.gameObject.SetActive(isCampaignSelected);
            
            cancelButton.gameObject.SetActive(_showCancelButton);
        }
    }
}
