﻿using System.Collections.Generic;
using Scripts.Building;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using LayoutType =
    System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>>;

namespace Scripts.MapEditor
{
    public class MapBuildService
    {
        private static MapEditorManager Manager => MapEditorManager.Instance;
        private LayoutType EditedLayout => Manager.EditedLayout;
        private MapBuilder MapBuilder => Manager.MapBuilder;
        private static EditorMouseService Mouse => EditorMouseService.Instance;
        private readonly Color _fullColor = new(1, 1, 1, 1);
        private readonly Color _nearTransparentColor = new(0.5f, 1, 0.5f, 0.65f);

        internal MapBuildService()
        {
        }

        public void ResetShownNullTilesColors()
        {
            foreach (GameObject shownNullTile in shownNullTiles)
            {
                shownNullTile.GetComponentInChildren<MeshRenderer>().material.color = _fullColor;
                shownNullTile.SetActive(false);
            }

            shownNullTiles.Clear();
        }

        private void AdjustEditedLayout(int floor, int row, int column,
            out int floorAdjustment, out int rowAdjustment, out int columnAdjustment, out bool wasAdjusted)
        {
            floorAdjustment = 0;
            rowAdjustment = 0;
            columnAdjustment = 0;
            int floorCount = EditedLayout.Count;
            int rowCount = EditedLayout[0].Count;
            int columnCount = EditedLayout[0][0].Count;
            wasAdjusted = false;

            // NW corner
            if (row == 0 && column == 0)
            {
                InsertColumnToStart();
                InsertRowToBack();

                rowAdjustment += 1;
                columnAdjustment += 1;
                wasAdjusted = true;
            }
            // NE corner
            else if (row == 0 && column == columnCount - 1)
            {
                AddColumn();
                InsertRowToBack();

                rowAdjustment += 1;
                wasAdjusted = true;
            }
            //SW corner
            else if (row == rowCount - 1 && column == 0)
            {
                InsertColumnToStart();
                AddRowToFront();

                columnAdjustment += 1;
                wasAdjusted = true;
            }
            // SE corner
            else if (row == rowCount - 1 && column == columnCount - 1)
            {
                AddColumn();
                AddRowToFront();
                wasAdjusted = true;
            }
            else if (row == 0)
            {
                InsertRowToBack();

                rowAdjustment += 1;
                wasAdjusted = true;
            }
            else if (column == 0)
            {
                InsertColumnToStart();

                columnAdjustment += 1;
                wasAdjusted = true;
            }
            else if (row == rowCount - 1)
            {
                AddRowToFront();
                wasAdjusted = true;
            }
            else if (column == columnCount - 1)
            {
                AddColumn();
                wasAdjusted = true;
            }
            else if (floor == 0)
            {
                InsertFloorToTop();
                floorAdjustment += 1;
                wasAdjusted = true;
            }
            else if (floor == floorCount - 1)
            {
                AddFloorToBottom();
                wasAdjusted = true;
            }
        }

        private void AddColumn()
        {
            foreach (List<List<TileDescription>> floor in EditedLayout)
            {
                foreach (List<TileDescription> row in floor)
                {
                    row.Add(null);
                }
            }
        }

        private void InsertColumnToStart()
        {
            foreach (List<List<TileDescription>> floor in EditedLayout)
            {
                foreach (List<TileDescription> row in floor)
                {
                    row.Insert(0, null);
                }
            }
        }

        private void InsertRowToBack()
        {
            foreach (List<List<TileDescription>> floor in EditedLayout)
            {
                floor.Insert(0, new List<TileDescription>());
            }

            PopulateRowOnAllFloors(0);
        }

        private void AddRowToFront()
        {
            foreach (List<List<TileDescription>> floor in EditedLayout)
            {
                floor.Add(new List<TileDescription>());
            }

            PopulateRowOnAllFloors(EditedLayout[0].Count - 1);
        }

        private void InsertFloorToTop()
        {
            EditedLayout.Insert(0, new List<List<TileDescription>>());

            PopulateFloor(0);
        }

        private void AddFloorToBottom()
        {
            EditedLayout.Add(new List<List<TileDescription>>());

            PopulateFloor(EditedLayout.Count - 1);
        }

        private void PopulateFloor(int index)
        {
            for (int row = 0; row < EditedLayout[1].Count; row++)
            {
                EditedLayout[index].Add(new List<TileDescription>());
            }

            foreach (List<TileDescription> row in EditedLayout[index])
            {
                for (int i = 0; i < EditedLayout[1][1].Count; i++)
                {
                    row.Add(null);
                }
            }
        }

        private void PopulateRowOnAllFloors(int index)
        {
            foreach (List<List<TileDescription>> floor in EditedLayout)
            {
                for (int row = 0; row < floor[0].Count; row++)
                {
                    floor[index].Add(null);
                }
            }
        }

        internal void ProcessBuildClick()
        {
            Vector3Int position = Mouse.MouseGridPosition;

            if (Mouse.LeftClickExpired 
                || Manager.WorkLevel == ELevel.Upper && (Mouse.GridPositionType != EGridPositionType.EditableTileAbove &&
                                                         Mouse.GridPositionType != EGridPositionType.NullTileAbove))
            {
                return;
            }

            Manager.MapIsChanged = true;
            Manager.MapIsSaved = false;
            
            ResetShownNullTilesColors();

            int floor = position.x;
            int row = position.y;
            int column = position.z;

            if (!Manager.EditedLayout.HasIndex(floor, row, column)) return;
            
            if (Manager.WorkLevel == ELevel.Upper)
            {
                floor -= 1;
            }

            AdjustEditedLayout(floor, row, column, out int floorAdjustment, out int rowAdjustment, out int columnAdjustment,
                out bool wasLayoutAdjusted);

            int adjustedFloor = floor + floorAdjustment;
            int adjustedRow = row + rowAdjustment;
            int adjustedColumn = column + columnAdjustment;

            EditedLayout[adjustedFloor][adjustedRow][adjustedColumn] =
                EditedLayout[adjustedFloor][adjustedRow][adjustedColumn] == null
                    ? DefaultMapProvider.FullTile
                    : null;

            MapDescription newMap = GameManager.Instance.CurrentMap;
            TileDescription[,,] newLayout = ConvertEditedLayoutToArray();
            newMap.Layout = newLayout;

            newMap.StartGridPosition = new Vector3Int(
                newMap.StartGridPosition.x + floorAdjustment,
                newMap.StartGridPosition.y + rowAdjustment,
                newMap.StartGridPosition.z + columnAdjustment);

            GameManager.Instance.SetCurrentMap(newMap);

            Mouse.RefreshMousePosition();

            if (!wasLayoutAdjusted)
            {
                MapBuilder.RebuildTile(adjustedFloor, adjustedRow, adjustedColumn);
                MapBuilder.RegenerateTilesAround(adjustedFloor, adjustedRow, adjustedColumn);
                return;
            }

            Manager.OrderMapConstruction(newMap, mapIsPresented: true);
        }

        private TileDescription[,,] ConvertEditedLayoutToArray()
        {
            TileDescription[,,] result = new TileDescription[EditedLayout.Count, EditedLayout[0].Count, EditedLayout[0][0].Count];

            for (int x = 0; x < EditedLayout.Count; x++)
            {
                for (int y = 0; y < EditedLayout[0].Count; y++)
                {
                    for (int z = 0; z < EditedLayout[0][0].Count; z++)
                    {
                        result[x, y, z] = EditedLayout[x][y][z];
                    }
                }
            }

            return result;
        }

        public static LayoutType ConvertToLayoutType(TileDescription[,,] layout)
        {
            LayoutType result = new();

            for (int floor = 0; floor < layout.GetLength(0); floor++)
            {
                result.Add(new List<List<TileDescription>>());

                for (int row = 0; row < layout.GetLength(1); row++)
                {
                    result[floor].Add(new List<TileDescription>());

                    for (int column = 0; column < layout.GetLength(2); column++)
                    {
                        result[floor][row].Add(layout[floor, row, column]);
                    }
                }
            }

            return result;
        }

        private HashSet<GameObject> shownNullTiles = new();

        public void ShowUpperLevelStoneCubesAround(Vector3Int centerGridPosition)
        {
            ResetShownNullTilesColors();

            SetColorForNullTile(centerGridPosition, _fullColor);

            foreach (Vector3Int direction in TileDirections.HorizontalGridDirections)
            {
                Vector3Int targetDirection = centerGridPosition + direction;

                SetColorForNullTile(targetDirection, _nearTransparentColor);
            }
        }

        private void SetColorForNullTile(Vector3Int griPosition, Color newColor)
        {
            if (!MapBuilder.Layout.HasIndex(griPosition)
                || MapBuilder.Layout.ByGridV3int(griPosition) != null)
                return;

            GameObject nullTile = MapBuilder.PhysicalTiles[griPosition.ToWorldPositionV3Int()];
            nullTile.GetComponentInChildren<MeshRenderer>().material.color = newColor;
            nullTile.SetActive(true);

            shownNullTiles.Add(nullTile);
        }
    }
}