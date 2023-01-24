using System;
using System.Collections.Generic;
using System.IO;
using Scripts.Building;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.Localization;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using Scripts.UI.EditorUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private void Awake()
        {
            Transform content = body.transform.Find("Background/Frame/Content");
            _campaignsParent = content.Find("CampaignsScrollView/Viewport/Content");
            _mapItemsParent = content.Find("MapsScrollView/Viewport/Content");
            _loadLastEditedMapButton = content.Find("LastEditedMapButton").GetComponent<Button>();
            _addCampaignButton = content.Find("AddCampaignButton").GetComponent<Button>();
            _addMapButton = content.Find("AddMapButton").GetComponent<Button>();
            _selectCampaignTitle = content.Find("SelectCampaignTitle").GetComponent<Title>();
            _selectMapTitle = content.Find("SelectMapTitle").GetComponent<Title>();
        }
        
        public async void Show(IEnumerable<Campaign> campaigns, Action<string> onFileItemClicked)
        {
            SetCommonComponents();
            _campaigns = campaigns;
            
            SetMapScrollView(onFileItemClicked);
            
            await base.Show(t.Get(Keys.MapSelection));
        }

        private void SetCommonComponents()
        {
            title.SetTitle(t.Get(Keys.MapSelection));
            // _selectCampaignTitle.SetTitle(t.Get(Keys.SelectCampaign));
            // _selectMapTitle.SetTitle(t.Get(Keys.SelectMap));
            // _loadLastEditedMapButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.LoadLastEditedMap);
            // _addCampaignButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.AddCampaign);
            // _addMapButton.GetComponentInChildren<TMP_Text>().text = t.Get(Keys.AddMap);
            _loadLastEditedMapButton.onClick.RemoveAllListeners();
            _addCampaignButton.onClick.RemoveAllListeners();
            _addMapButton.onClick.RemoveAllListeners();
            // _loadLastEditedMapButton.onClick.AddListener(() => OnFileItemClicked(FileOperationsHelper.GetLastEditedMapKey()));
            // _addCampaignButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddCampaignKey()));
            // _addMapButton.onClick.AddListener(() => FileOperations.(FileOperationsHelper.GetAddMapKey()));
        }

        private void SetMapScrollView(Action<string> onFileItemClicked)
        {
            foreach (Button button in _mapItemsParent.GetComponentsInChildren<Button>())
            {
                button.onClick.RemoveAllListeners();
                ObjectPool.Instance.ReturnToPool(button.transform.parent.gameObject);
            }

            foreach (MapDescription map in _selectedCampaign.Maps)  
            {
                GameObject fileItem = ObjectPool.Instance.GetFromPool(fileItemPrefab, _mapItemsParent.gameObject);
                fileItem.GetComponentInChildren<TMP_Text>().text = map.MapName;
                fileItem.GetComponentInChildren<Button>().onClick.AddListener(() => onFileItemClicked?.Invoke(FileOperationsHelper.GetCampaignMapKey(_selectedCampaign.CampaignName, map.MapName)));
            }
        }
    }
}
