using System;
using UnityEngine;
using UnityEngine.EventSystems;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class EditorInputService : MonoBehaviour
    {
        private static MapEditorManager Manager => MapEditorManager.Instance;
        private MapBuildService _buildService;

        private void Awake()
        {
            _buildService = new MapBuildService();
        }

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            if (Manager.MapIsPresented)
            {
                if (Input.GetMouseButtonDown(0))
                    ProcessMouseButtonDown(0);
                if (Input.GetMouseButtonUp(0))
                    ProcessMouseButtonUp(0);
                if (Input.GetMouseButtonUp(1))
                    ProcessMouseButtonUp(1);
            }
        }
        
        private void ProcessMouseButtonDown(int mouseButtonDowned)
        {
            if (mouseButtonDowned == 0)
            {
                // TODO is something pop up which needs doing
            }
        }

        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            switch (Manager.WorkMode)
            {
                case Enums.EWorkMode.None:
                    break;
                case Enums.EWorkMode.Build:
                    if (mouseButtonUpped == 0)
                    {
                        _buildService.ProcessBuildClick();
                    }

                    break;
                case Enums.EWorkMode.Select:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}