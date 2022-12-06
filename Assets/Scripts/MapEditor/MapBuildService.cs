using System.Collections.Generic;
using Scripts.Building;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class MapBuildService
    {
        private MapEditorManager Manager => MapEditorManager.Instance;
        private LayoutType EditedLayout => Manager.EditedLayout;
        private MapBuilder MapBuilder => Manager.MapBuilder;
        private EditorMouseService Mouse => EditorMouseService.Instance;

        internal MapBuildService()
        {
            
        }
        
        internal void AdjustEditedLayout(int row, int column, out int adjustedX, out int adjustedY, out bool wasAdjusted)
        {
            int rowAdjustment = 0;
            int columnAdjustment = 0;
            int rowCount = EditedLayout.Count;
            int columnCount = EditedLayout[0].Count;
            wasAdjusted = false;
            
            // NW corner
            if (row == 0 && column == 0)
            {
                InsertColumnToStart();
                InsertRowToTop();

                rowAdjustment += 1;
                columnAdjustment += 1;
                wasAdjusted = true;
            }
            // NE corner
            else if (row == 0 && column == columnCount - 1)
            {
                AddColumn();
                InsertRowToTop();

                rowAdjustment += 1;
                wasAdjusted = true;
            }
            //SW corner
            else if (row == rowCount - 1 && column == 0)
            {
                InsertColumnToStart();
                AddRowToBottom();

                columnAdjustment += 1;
                wasAdjusted = true;
            }
            // SE corner
            else if (row == rowCount - 1 && column == columnCount - 1)
            {
                AddColumn();
                AddRowToBottom();
                wasAdjusted = true;
            }
            else if (row == 0)
            {
                InsertRowToTop();

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
                AddRowToBottom();
                wasAdjusted = true;
            }
            else if (column == columnCount - 1)
            {
                AddColumn();
                wasAdjusted = true;
            }
            
            adjustedX = rowAdjustment;
            adjustedY = columnAdjustment;
        }
        
        private void AddColumn()
        {
            foreach (List<TileDescription> r in EditedLayout)
            {
                r.Add(null);
            }
        }

        private void InsertColumnToStart()
        {
            foreach (List<TileDescription> r in EditedLayout)
            {
                r.Insert(0, null);
            }
        }

        private void InsertRowToTop()
        {
            EditedLayout.Insert(0, new List<TileDescription>());

            PopulateRow(0);
        }

        private void AddRowToBottom()
        {
            EditedLayout.Add(new List<TileDescription>());

            PopulateRow(EditedLayout.Count - 1);
        }

        private void PopulateRow(int index)
        {
            for (int i = 0; i < EditedLayout[1].Count; i++)
            {
                EditedLayout[index].Add(null);
            }
        }
        
        internal void ProcessBuildClick()
        {
            Vector3Int position = Mouse.MouseGridPosition;
            
            if (Mouse.LastGridMouseDownPosition != position) return;

            Manager.MapIsChanged = true;
            
            int row = position.x;
            int column = position.z;
            
            if (!Manager.EditedLayout.HasIndex(row, column)) return;
            
            Enums.EGridPositionType tileType = EditorMouseService.Instance.GridPositionType;

            AdjustEditedLayout(row, column, out int rowAdjustment, out int columnAdjustment, out bool wasLayoutAdjusted);

            int adjustedRow = row + rowAdjustment;
            int adjustedColumn = column + columnAdjustment;

            EditedLayout[adjustedRow][adjustedColumn] = tileType == Enums.EGridPositionType.Null 
                ? DefaultMapProvider.FullTile 
                : null;
            
            MapDescription newMap = GameController.Instance.CurrentMap;
            TileDescription[,] newLayout = ConvertEditedLayoutToArray();
            newMap.Layout = newLayout;
            newMap.StartPosition = new Vector3Int(newMap.StartPosition.x + rowAdjustment, 0, newMap.StartPosition.z + columnAdjustment);
            GameController.Instance.SetCurrentMap(newMap);

            Mouse.RefreshMousePosition();
            
            if (!wasLayoutAdjusted)
            {
                MapBuilder.RebuildTile(adjustedRow, adjustedColumn);
                MapBuilder.RegenerateTilesAround(adjustedRow, adjustedColumn);
                return;
            }
            
            Manager.OrderMapConstruction(newMap);
        }
        
        public TileDescription[,] ConvertEditedLayoutToArray()
        {
            TileDescription[,] result = new TileDescription[EditedLayout.Count, EditedLayout[0].Count];

            for (int x = 0; x < EditedLayout.Count; x++)
            {
                for (int y = 0; y < EditedLayout[0].Count; y++)
                {
                    result[x, y] = EditedLayout[x][y];
                }
            }

            return result;
        }

        public static LayoutType ConvertToLayoutType(TileDescription[,] layout)
        {
            LayoutType result = new();

            for (int x = 0; x < layout.GetLength(0); x++)
            {
                result.Add(new List<TileDescription>());

                for (int y = 0; y < layout.GetLength(1); y++)
                {
                    result[x].Add(layout[x, y]);
                }
            }

            return result;
        }
    }
}