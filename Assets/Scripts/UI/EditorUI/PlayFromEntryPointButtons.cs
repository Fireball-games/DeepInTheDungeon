using Scripts.Building;
using Scripts.EventsManagement;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.MonoBases;
using Scripts.System.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.EditorUI
{
    public class PlayFromEntryPointButtons : UIElementBase
    {
        [SerializeField] private Button buttonPrefab;

        private static MapBuilder MapBuilder
        {
            get
            {
                if (GameManager.Instance == null || GameManager.Instance.MapBuilder == null)
                    return null;
                
                return GameManager.Instance.MapBuilder;
            }
        }

        private void OnEnable()
        {
            MapBuilder.OnLayoutBuilt.AddListener(RedrawButtons);
            EditorEvents.OnMapSaved += RedrawButtons;
        }

        private void OnDisable()
        {
            if (MapBuilder == null) return;
            
            MapBuilder.OnLayoutBuilt.RemoveListener(RedrawButtons);
            EditorEvents.OnMapSaved -= RedrawButtons;
        }
        
        private void RedrawButtons()
        {
            ClearButtons();
            CreateButtons();
        }
        
        private void ClearButtons()
        {
            gameObject.DismissAllChildrenToPool();
        }
        
        private void CreateButtons()
        {
            foreach (EntryPoint entryPoint in GameManager.Instance.CurrentMap.EntryPoints)
            {
                Button button = ObjectPool.Instance.GetFromPool(buttonPrefab.gameObject, gameObject).GetComponent<Button>();
                button.GetComponentInChildren<TMP_Text>().text = entryPoint.name;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => MapEditorManager.Instance.PlayFromEntryPoint(entryPoint));
            }
        }
    }
}