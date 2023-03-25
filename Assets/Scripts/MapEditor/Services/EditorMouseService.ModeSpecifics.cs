using System;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.Tile;
using Scripts.Helpers.Extensions;
using Scripts.System;
using UnityEngine;
using static Scripts.Enums;
using static Scripts.MapEditor.Enums;
using static Scripts.System.MouseCursorManager;

namespace Scripts.MapEditor.Services
{
    public partial class EditorMouseService
    {
        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            LastLeftButtonUpWorldPosition = MouseGridPosition.ToWorldPositionV3Int();

            switch (Manager.WorkMode)
            {
                case EWorkMode.None:
                    break;
                case EWorkMode.Build:
                    if (mouseButtonUpped == 0 && GridPositionType != EGridPositionType.None)
                    {
                        _buildService.ProcessBuildTileClick();
                    }

                    break;
                case EWorkMode.Select:
                    break;
                case EWorkMode.Walls:
                    if (mouseButtonUpped == 0 && GridPositionType != EGridPositionType.None)
                    {
                        if (_lastEnteredWall is {WallEligibleForEditing: true}) _lastEnteredWall.OnClickInEditor();
                    }

                    break;
                case EWorkMode.PrefabTiles:
                    OpenEditorForTiledPrefab<TilePrefab>(mouseButtonUpped, EPrefabType.PrefabTile);
                    break;
                case EWorkMode.Prefabs:
                case EWorkMode.Items:
                case EWorkMode.Enemies:
                case EWorkMode.Triggers:
                    break;
                case EWorkMode.TriggerReceivers:
                    break;
                case EWorkMode.SetWalls:
                    break;
                case EWorkMode.EditEntryPoints:
                    OpenEditorForTiledPrefab<EntryPointPrefab>(mouseButtonUpped, EPrefabType.Service);
                    break;
                case EWorkMode.EditEditorStart:
                    if (mouseButtonUpped == 0 && GridPositionType == EGridPositionType.EditableTile)
                    {
                        UIManager.OpenEditorWindow(EPrefabType.Service,
                            new PositionRotation(MouseGridPosition.ToWorldPositionV3Int(),
                                Quaternion.identity));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetCursorByCurrentType()
        {
            switch (Manager.WorkMode)
            {
                case EWorkMode.Walls or EWorkMode.Triggers:
                    SetDefaultCursor();
                    break;
                case EWorkMode.PrefabTiles:
                    SetCursorForTiledPrefabType<TilePrefab>();
                    break;
                case EWorkMode.EditEntryPoints:
                    SetCursorForTiledPrefabType<EntryPointPrefab>();
                    break;
                case EWorkMode.Items:
                    SetCursorForItemsMode();
                    break;
                case EWorkMode.EditEditorStart:
                    SetCursorForEditEditorStartMode();
                    break;
                case EWorkMode.Build:
                    SetCursorForBuildMode();
                    break;
            }
        }

        private void SetCursorForItemsMode()
        {
            if (GridPositionType is EGridPositionType.NullTile)
            {
                SetDefaultCursor();
            }
            else
            {
                SetCursor(Manager.EditMode is EEditMode.Edit ? ECursorType.Default : ECursorType.Hidden);
            }
        }

        private void SetCursorForEditEditorStartMode()
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

        private void SetCursorForBuildMode()
        {
            switch (GridPositionType)
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

        private void SetCursorForTiledPrefabType<TPrefab>() where TPrefab : PrefabBase
        {
            GameObject prefabOnPosition = Manager.MapBuilder.GetPrefabByGridPosition(MouseGridPosition);
            bool isDesiredPrefab = prefabOnPosition && prefabOnPosition.GetComponent<TPrefab>();

            if (GridPositionType == EGridPositionType.NullTile || prefabOnPosition && !isDesiredPrefab)
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