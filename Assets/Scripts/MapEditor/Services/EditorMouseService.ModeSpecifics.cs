using Scripts.Building.PrefabsSpawning;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MouseCursorManager;

namespace Scripts.MapEditor.Services
{
    public partial class EditorMouseService
    {
        private void SetCursorByType(EGridPositionType type)
        {
            EWorkMode workMode = Manager.WorkMode;
            
            switch (workMode)
            {
                case EWorkMode.Walls or EWorkMode.Triggers:
                    SetDefaultCursor();
                    break;
                case EWorkMode.PrefabTiles:
                    SetCursorForTiledPrefabType<TilePrefab>(type);
                    break;
                case EWorkMode.EditEntryPoints:
                    SetCursorForTiledPrefabType<EntryPointPrefab>(type);
                    break;
                case EWorkMode.Items:
                    SetCursorForItemsMode(type);
                    break;
                case EWorkMode.EditEditorStart:
                    SetCursorForEditEditorStartMode(type);
                    break;
                case EWorkMode.Build:
                    SetCursorForBuildMode(type);
                    break;
            }
        }

        private void SetCursorForItemsMode(EGridPositionType type)
        {
            if (type == EGridPositionType.NullTile)
            {
                SetDefaultCursor();
            }
            else
            {
                Show3DCursor(MouseGridPosition);
                SetCursor(ECursorType.Add);
            }
        }

        private void SetCursorForEditEditorStartMode(EGridPositionType type)
        {
            if (GridPositionType is EGridPositionType.NullTile)
            {
                SetDefaultCursor();
            }
            else
            {
                Show3DCursor(MouseGridPosition);
                SetCursor(ECursorType.Move);
            }
        }

        private void SetCursorForBuildMode(EGridPositionType type)
        {
            switch (type)
            {
                case EGridPositionType.None:
                    Hide3DCursor();
                    SetDefaultCursor();
                    break;
                case EGridPositionType.NullTile:
                    Show3DCursor(MouseGridPosition);
                    SetCursor(ECursorType.Build);
                    break;
                case EGridPositionType.EditableTile:
                    Show3DCursor(MouseGridPosition);
                    SetCursor(ECursorType.Demolish);
                    break;
                case EGridPositionType.NullTileAbove:
                    Show3DCursor(MouseGridPosition, true);
                    SetCursor(ECursorType.Build);
                    break;
                case EGridPositionType.EditableTileAbove:
                    Show3DCursor(MouseGridPosition, true);
                    SetCursor(ECursorType.Demolish);
                    break;
                case EGridPositionType.NullTileBellow:
                    Show3DCursor(MouseGridPosition, withCopyBellow: true);
                    SetCursor(ECursorType.Build);
                    break;
                case EGridPositionType.EditableTileBellow:
                    Show3DCursor(MouseGridPosition, withCopyBellow: true);
                    SetCursor(ECursorType.Demolish);
                    break;
                default:
                    ResetCursor();
                    break;
            }
        }
        
        private void SetCursorForTiledPrefabType<TPrefab>(EGridPositionType type) where TPrefab : PrefabBase
        {
            GameObject prefabOnPosition = Manager.MapBuilder.GetPrefabByGridPosition(MouseGridPosition);
            bool isDesiredPrefab = prefabOnPosition && prefabOnPosition.GetComponent<TPrefab>();
            
            if (type == EGridPositionType.NullTile || prefabOnPosition && !isDesiredPrefab)
            {
                _lastPrefabOnPosition = null;
                cursor3D.Hide();
                SetCursor(ECursorType.Default);
                return;
            }

            if (prefabOnPosition)
            {
                _lastPrefabOnPosition = prefabOnPosition;
                    
                if (isDesiredPrefab)
                {
                    SetCursor(ECursorType.Edit);
                    Show3DCursor(MouseGridPosition);
                }
            }
            else
            {
                _lastPrefabOnPosition = null;
                SetCursor(ECursorType.Add);
                Show3DCursor(MouseGridPosition);
            }
        }
        
        private void ResolveBuildModePosition(bool isNullTile, Vector3Int newGridPosition,
            TileDescription[,,] layout)
        {
            if (Manager.WorkMode != EWorkMode.Build) return;

            if (Manager.WorkLevel == ELevel.Equal)
            {
                upperFloorTrigger.SetActive(false);
                _buildService.HandleUpperFloorVisibility();

                GridPositionType = isNullTile ? EGridPositionType.NullTile : EGridPositionType.EditableTile;

                if (IsPositionOccupied(newGridPosition))
                {
                    GridPositionType = EGridPositionType.None;
                }
            }
            else if (Manager.WorkLevel == ELevel.Upper)
            {
                if (!isNullTile)
                {
                    Vector3Int aboveGridPosition = newGridPosition.AddToX(-1);

                    if (IsPositionOccupied(aboveGridPosition))
                    {
                        GridPositionType = EGridPositionType.None;
                        return;
                    }

                    bool isNullTileAbove = layout.ByGridV3Int(aboveGridPosition) == null;

                    GridPositionType = isNullTileAbove ? EGridPositionType.NullTileAbove : EGridPositionType.EditableTileAbove;

                    upperFloorTrigger.transform.position = MouseGridPosition.ToWorldPosition();
                    upperFloorTrigger.SetActive(true);
                }
                else
                {
                    upperFloorTrigger.SetActive(false);

                    _buildService.HandleUpperFloorVisibility();

                    GridPositionType = EGridPositionType.None;
                }
            }
            else if (Manager.WorkLevel == ELevel.Lower)
            {
                upperFloorTrigger.SetActive(false);
                _buildService.HandleUpperFloorVisibility();

                if (!isNullTile)
                {
                    Vector3Int bellowGridPosition = newGridPosition.AddToX(1);

                    if (IsPositionOccupied(bellowGridPosition))
                    {
                        GridPositionType = EGridPositionType.None;
                        return;
                    }

                    bool isNullTileBellow = layout.ByGridV3Int(bellowGridPosition) == null;

                    GridPositionType = isNullTileBellow ? EGridPositionType.NullTileBellow : EGridPositionType.EditableTileBellow;
                }
                else
                {
                    GridPositionType = EGridPositionType.None;
                }
            }
        }
    }
}