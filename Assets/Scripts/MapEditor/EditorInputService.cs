using System;
using UnityEngine;
using UnityEngine.EventSystems;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class EditorInputService : MonoBehaviour
    {
        private MapEditorManager _manager;
        private MapBuildService _buildService;

        private void Awake()
        {
            _manager = MapEditorManager.Instance;
            _buildService = new MapBuildService(_manager);
        }

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            if (_manager.MapIsEdited)
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
                EditorMouseService.Instance.SetLastMouseDownPosition();
            }
        }

        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            switch (_manager.WorkMode)
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