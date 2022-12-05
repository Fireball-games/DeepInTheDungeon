using System;
using System.Collections.Generic;
using Scripts.Building;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.System;
using UnityEngine;
using static Scripts.MapEditor.Enums;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.MapEditor
{
    public class MapEditorManager : Singleton<MapEditorManager>
    {
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private PlayerIconController playerIcon;

        public EWorkMode WorkMode => _workMode;
        public bool MapIsEdited { get; private set; }
        public bool MapIsBeingBuilt { get; private set; }

        private MapBuilder _mapBuilder;
        private EWorkMode _workMode;

        public LayoutType EditedLayout { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            sceneCamera ??= Camera.main;
            CameraManager.Instance.SetMainCamera(sceneCamera);

            _mapBuilder ??= GameController.Instance.MapBuilder;
        }

        private void OnEnable()
        {
            _mapBuilder.OnLayoutBuilt += OnLayoutBuilt;
        }

        private void Update()
        {
            if (MapIsEdited)
            {
                if (Input.GetMouseButtonUp(0))
                    ProcessMouseButtonUp(0);
                if (Input.GetMouseButtonUp(1))
                    ProcessMouseButtonUp(1);
            }
        }

        private void OnDisable()
        {
            _mapBuilder.OnLayoutBuilt -= OnLayoutBuilt;
        }

        public void OrderMapConstruction(MapDescription map = null)
        {
            MapIsBeingBuilt = true;
            MapIsEdited = false;

            if (_mapBuilder.LayoutParent) _mapBuilder.DemolishMap();

            MapDescription newMap = map ??= new MapDescription();

            EditedLayout = ConvertToLayoutType(map.Layout);

            GameController.Instance.SetCurrentMap(newMap);

            _mapBuilder.BuildMap(newMap);

            EditorEvents.TriggerOnNewMapCreated();
        }

        public void SetWorkMode(EWorkMode newWorkMode)
        {
            _workMode = newWorkMode;
            EditorEvents.TriggerOnWorkModeChanged(_workMode);
        }

        private void OnLayoutBuilt()
        {
            MapIsBeingBuilt = false;
            MapIsEdited = true;
            SetWorkMode(EWorkMode.Build);
            Vector3 startPosition = GameController.Instance.CurrentMap.StartPosition;
            sceneCamera.transform.position = new Vector3(startPosition.x, cameraHeight, startPosition.z);
            playerIcon.transform.position = GameController.Instance.CurrentMap.StartPosition;
            // TODO: rotate by data from CurrentMap when implemented
            playerIcon.SetArrowRotation(Vector3.zero);
            playerIcon.SetActive(true);
        }

        private TileDescription[,] ConvertToArrayLayout()
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

        private LayoutType ConvertToLayoutType(TileDescription[,] layout)
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

        #region Input Processing

        private void ProcessMouseButtonUp(int mouseButtonUpped)
        {
            switch (WorkMode)
            {
                case EWorkMode.None:
                    break;
                case EWorkMode.Build:
                    if (mouseButtonUpped == 0)
                    {
                        ProcessBuildClick();
                    }

                    break;
                case EWorkMode.Select:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessBuildClick()
        {
            
            Vector3Int position = EditorMouseService.Instance.MouseGridPosition;
            int row = position.x;
            int column = position.z;
            
            if (!EditedLayout.HasIndex(row, column)) return;
            
            int rowAdjustment = 0;
            int columnAdjustment = 0;
            int rowCount = EditedLayout.Count;
            int columnCount = EditedLayout[0].Count;
            EGridPositionType tileType = EditorMouseService.Instance.GridPositionType;

            // NW corner
            if (row == 0 && column == 0)
            {
                InsertColumnToStart();
                InsertRowToTop();

                rowAdjustment += 1;
                columnAdjustment += 1;
            }
            // NE corner
            else if (row == 0 && column == columnCount - 1)
            {
                AddColumn();
                InsertRowToTop();

                rowAdjustment += 1;
            }
            //SW corner
            else if (row == rowCount - 1 && column == 0)
            {
               InsertColumnToStart();
               AddRowToBottom();

                columnAdjustment += 1;
            }
            // SE corner
            else if (row == rowCount - 1 && column == columnCount - 1)
            {
                AddColumn();
                AddRowToBottom();
            }
            else if (row == 0)
            {
                InsertRowToTop();

                rowAdjustment += 1;
            }
            else if (column == 0)
            {
                InsertColumnToStart();

                columnAdjustment += 1;
            }
            else if (row == rowCount - 1)
            {
                AddRowToBottom();
            }
            else if (column == columnCount - 1)
            {
                AddColumn();
            }

            EditedLayout[row + rowAdjustment][column + columnAdjustment] = tileType == EGridPositionType.Null 
                // TODO: add proper wall setup based on surrounding tiles
                ? DefaultMapProvider.FullTile 
                : null;

            MapDescription newMap = GameController.Instance.CurrentMap;
            newMap.Layout = ConvertToArrayLayout();
            newMap.StartPosition = new(newMap.StartPosition.x + rowAdjustment, 0, newMap.StartPosition.z + columnAdjustment);

            OrderMapConstruction(newMap);
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

        #endregion
    }
}