using System.Collections.Generic;
using Scripts.Building.Tile;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class MapBuildService
    {
        private readonly MapEditorManager _manager;
        private LayoutType EditedLayout => _manager.EditedLayout;

        internal MapBuildService(MapEditorManager manager)
        {
            _manager = manager;
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
    }
}