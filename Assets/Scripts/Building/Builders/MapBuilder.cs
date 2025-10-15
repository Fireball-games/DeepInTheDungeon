using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts.Building.ItemSpawning;
using Scripts.Building.PrefabsBuilding;
using Scripts.Building.PrefabsSpawning;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.EventsManagement;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using UnityEngine.Events;
using static Scripts.Enums;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Building
{
    public class MapBuilder : MonoBehaviour
    {
        public DefaultBuildPartsProvider defaultsProvider;
        [SerializeField] private GameObject levelPartsParent;

        private TileBuilderBase _playBuilder;
        private TileBuilderBase _editorBuilder;

        public UnityEvent OnLayoutBuilt { get; } = new();

        internal Transform LayoutParent;
        internal TileDescription[,,] Layout;
        internal Dictionary<Vector3Int, GameObject> PhysicalTiles;
        internal Dictionary<int, List<NullTile>> NullTilesMap;
        internal MapDescription MapDescription;
        internal GameObject PrefabsParent;
        internal GameObject ItemsParent;

        private static MapEditorManager EditorManager => MapEditorManager.Instance;
        private PrefabBuilder _prefabBuilder;
        private ItemSpawner _itemSpawner;
        internal HashSet<GameObject> Prefabs;
        
        public static bool IsCurrentMapOutdoor { get; private set; }

        private void Awake()
        {
            PhysicalTiles = new Dictionary<Vector3Int, GameObject>();
            NullTilesMap = new Dictionary<int, List<NullTile>>();
            Prefabs = new HashSet<GameObject>();
            _prefabBuilder = new PrefabBuilder();
            _itemSpawner = new ItemSpawner();

            if (!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                PrefabsParent = new GameObject("Prefabs");
                ItemsParent = new GameObject("Items");

                LayoutParent.transform.parent = PrefabsParent.transform.parent =
                    ItemsParent.transform.parent = levelPartsParent.transform;
            }
        }

        public void BuildMap(MapDescription mapDescription)
        {
            IsCurrentMapOutdoor = mapDescription.IsOutdoor;
            StartCoroutine(BuildMapCoroutine(mapDescription));
        }

        public void SetLayout(TileDescription[,,] layout) => Layout = layout;

        public void DemolishMap()
        {
            PhysicalTiles.Values.ForEach(ObjectPool.Instance.Dismiss);
            _itemSpawner.DemolishItems();

            foreach (GameObject prefab in Prefabs)
            {
                Transform offsetTransform = prefab.GetBody();

                if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

                ObjectPool.Instance.Dismiss(prefab);
            }

            NullTilesMap.Clear();
            PhysicalTiles.Clear();
            Prefabs.Clear();

            EventsManager.TriggerOnMapDemolished();
        }

        /// <summary>
        /// Build a new tile where was previously null tile. Or null tile where was previously a tile.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="floor"></param>
        public void RebuildTile(int floor, int row, int column)
        {
            if (GameManager.IsInPlayMode)
            {
                _playBuilder.BuildTile(floor, row, column);
            }
            else
            {
                // Logger.Log($"Rebuilding tile: {floor},{row},{column}");
                _editorBuilder.BuildTile(floor, row, column);
            }
        }

        public void RegenerateTilesAround(int floor, int row, int column)
        {
            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                if (Layout[floor + direction.y, row + direction.x, column + direction.z] != null)
                {
                    RegenerateTile(floor + direction.y, row + direction.x, column + direction.z);
                }
            }
        }

        public Campaign GenerateFallbackStartRoomsCampaign()
        {
            MapDescription defaultMap = GenerateFallbackStartRoomsMap(3, 5, 5);

            Campaign defaultCampaign = new()
            {
                CampaignName = Strings.StartRoomsCampaignName,
                StartMapName = defaultMap.MapName,
                Maps = new List<MapDescription> {defaultMap},
            };

            return defaultCampaign;
        }

        public static MapDescription GenerateFallbackStartRoomsMap(int floors, int rows, int columns)
        {
            TileDescription[,,] layout = new TileDescription[floors, rows, columns];

            Vector3Int center = new(floors / 2, rows / 2, columns / 2);

            layout = AddTilesToCenterOfLayout(layout);

            return new MapDescription
            {
                Layout = layout,
                EditorStartPosition = center,
                EditorPlayerStartRotation = Quaternion.identity,
                SceneName = Scenes.PlayIndoorSceneName,
            };
        }

        public static MapDescription GenerateOutdoorMap(int rows, int columns)
        {
            int adjustedRows = rows + 2;
            int adjustedColumns = columns + 2;
            // Outdoor map have just ground level, but we add one more level above in order to have a start area.
            // Surrounded by null tiles, so player can't go out of the map.
            TileDescription[,,] layout = new TileDescription[3, adjustedRows, adjustedColumns];

            CreateOutdoorBaseLayout(adjustedRows, adjustedColumns, layout);

            return new MapDescription
            {
                Layout = layout,
                EditorStartPosition = new Vector3Int(1, adjustedRows / 2, adjustedColumns / 2),
                EditorPlayerStartRotation = Quaternion.identity,
                SceneName = Scenes.PlayOutdoorSceneName,
                GroundIndex = 1,
            };
        }

        private static void CreateOutdoorBaseLayout(int adjustedRows, int adjustedColumns, TileDescription[,,] layout)
        {
            //Above ground level is surrounded by null tiles
            for (int r = 0; r < adjustedRows; r++)
            {
                for (int c = 0; c < adjustedColumns; c++)
                {
                    //Above ground are null tiles
                    layout[0, r, c] = null;

                    // Levels above ground are full of walkable tiles, except borders
                    if (r == 0 || r == adjustedRows - 1 || c == 0 || c == adjustedColumns - 1)
                    {
                        layout[1, r, c] = null;
                    }
                    else // Not the edge
                    {
                        layout[1, r, c] = DefaultMapProvider.FullTile;
                    }

                    // Below ground level is full of null tiles (ground)
                    layout[2, r, c] = null;
                }
            }
        }

        public GameObject GetPhysicalTileByGridPosition(int floor, int row, int column)
            => GetPhysicalTileByWorldPosition(new Vector3(row, -floor, column).ToVector3Int());

        public GameObject GetPhysicalTileByWorldPosition(Vector3 worldPosition) =>
            PhysicalTiles[worldPosition.ToVector3Int()];

        /// <summary>
        /// Determinate if floor should be visible, usable only from Editor
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public bool ShouldBeInvisible(int floor)
        {
            bool isFloorInMap = EditorManager.FloorVisibilityMap.ContainsKey(floor);
            return !isFloorInMap || !EditorManager.FloorVisibilityMap[floor];
        }

        public void SetPrefabsVisibility() => SetPrefabsVisibility(EditorManager.FloorVisibilityMap);

        public void SetPrefabsVisibility(Dictionary<int, bool> floorVisibilityMap)
        {
            foreach (GameObject prefab in Prefabs)
            {
                prefab.SetActive(floorVisibilityMap[Mathf.RoundToInt(-prefab.transform.position.y)]);
            }
        }

        public void SetTileForMovement(Vector3 worldPosition, bool isWalkable)
            => Layout.ByGridV3Int(worldPosition.ToGridPosition()).IsForMovement = isWalkable;

        private IEnumerator BuildMapCoroutine(MapDescription mapDescription)
        {
            DemolishMap();

            MapDescription = mapDescription;
            // TODO: Try to convert this to async/await, just mind, that ProcessPostBuildLayoutPrefabs() must run after layout is build
            yield return StartCoroutine(BuildLayoutCoroutine(mapDescription.Layout));
            yield return _prefabBuilder.BuildPrefabs(mapDescription.PrefabConfigurations);
            yield return StartCoroutine(_prefabBuilder.ProcessPostBuildLayoutPrefabs());
            yield return _itemSpawner.SpawnItemsAsync(mapDescription.MapObjects);

            OnLayoutBuilt?.Invoke();
        }

        private int _runningFloorBuilds;

        private IEnumerator BuildLayoutCoroutine(TileDescription[,,] layout)
        {
            Layout = layout;

            _playBuilder = new PlayModeBuilder(this);
            _editorBuilder = new EditorModeBuilder(this);

            for (int floor = 0; floor < layout.GetLength(0); floor++)
            {
                _runningFloorBuilds += 1;
                StartCoroutine(BuildFloor(floor, layout));
            }

            yield return new WaitUntil(() => _runningFloorBuilds == 0);
        }

        private IEnumerator BuildFloor(int floor, TileDescription[,,] layout)
        {
            for (int row = 0; row < layout.GetLength(1); row++)
            {
                _runningRowBuilds += 1;
                StartCoroutine(BuildRow(floor, row, layout));
            }

            yield return new WaitUntil(() => _runningRowBuilds == 0);

            _runningFloorBuilds -= 1;
        }

        private int _runningRowBuilds;

        private IEnumerator BuildRow(int floor, int row, TileDescription[,,] layout)
        {
            for (int column = 0; column < layout.GetLength(2); column++)
            {
                if (GameManager.IsInPlayMode)
                {
                    _playBuilder.BuildTile(floor, row, column);
                }
                else
                {
                    _editorBuilder.BuildTile(floor, row, column);
                }

                yield return null;
            }

            _runningRowBuilds -= 1;
        }

        /// <summary>
        /// Works over physical tile, shows or hides walls after assumed changed layout. 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="floor"></param>
        private void RegenerateTile(int floor, int row, int column)
        {
            Vector3Int worldKey = new(row, -floor, column);

            TileController tileController = PhysicalTiles[worldKey].GetComponent<TileController>();

            if (!tileController)
            {
                return;
            }

            foreach (Vector3Int direction in TileDirections.VectorDirections)
            {
                int x = floor + direction.y;
                int y = row + direction.x;
                int z = column + direction.z;
                Vector3Int gridInDirection = new(x, y, z);
                
                if (Layout[x, y, z] == null && !IsOutdoorEdgeNullTile(gridInDirection))
                    tileController.ShowWall(TileDirections.WallDirectionByVector[direction]);
                else
                    tileController.HideWall(TileDirections.WallDirectionByVector[direction]);
            }

            if (GameManager.IsInEditMode)
            {
                tileController.HideWall(TileDescription.ETileDirection.Ceiling);
            }
        }

        private static TileDescription[,,] AddTilesToCenterOfLayout(TileDescription[,,] layout)
        {
            Vector2Int center = new(layout.GetLength(1) / 2, layout.GetLength(2) / 2);
            int floor = layout.GetLength(0) / 2;

            layout[floor, center.x - 1, center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x - 1, center.y + 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x - 1, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x, center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x, center.y + 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y - 1] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y] = DefaultMapProvider.FullTile;
            layout[floor, center.x + 1, center.y + 1] = DefaultMapProvider.FullTile;

            return layout;
        }
        
        public bool IsOutdoorEdgeNullTile(Vector3Int gridDirection)
        {
            return MapDescription.IsOutdoor 
                   && IsOnOrAboveGroundLevel(gridDirection.x) 
                   && IsEdgeTile(gridDirection);
        }

        public bool IsEdgeTile(Vector3Int gridPosition)
        {
            int floors = Layout.GetLength(0);
            
            // Check if the position is on the top or bottom floor
            bool onFloorEdge = (gridPosition.x == 0) || (gridPosition.x == floors - 1);
            
            return onFloorEdge || IsHorizontalEdgeTile(gridPosition);
        }
        
        public bool IsHorizontalEdgeTile(Vector3Int gridPosition)
        {
            int rows = Layout.GetLength(1);
            int columns = Layout.GetLength(2);

            // Check if the position is on the row, or column boundaries
            bool onRowEdge = (gridPosition.y == 0) || (gridPosition.y == rows - 1);
            bool onColumnEdge = (gridPosition.z == 0) || (gridPosition.z == columns - 1);
            
            return onRowEdge || onColumnEdge;
        }
        
        public bool IsColumnSliceEdgeTile(Vector3Int gridPosition)
        {
            int floors = Layout.GetLength(0);
            int rows = Layout.GetLength(1);
            // Check if the position is on the column boundaries
            bool floorEdge = (gridPosition.x == 0) || (gridPosition.x == floors - 1);
            bool rowEdge = (gridPosition.y == 0) || (gridPosition.y == rows - 1);
            
            return floorEdge || rowEdge;
        }
        
        public bool IsRowSliceEdgeTile(Vector3Int gridPosition, List<List<List<TileDescription>>> layout)
        {
            int floors = layout.Count;
            int columns = layout[0][0].Count;
            // Check if the position is on the row boundaries
            bool floorEdge = (gridPosition.x == 0) || (gridPosition.x == floors - 1);
            bool columnEdge = (gridPosition.z == 0) || (gridPosition.z == columns - 1);
            
            return floorEdge || columnEdge;
        }
        
        public bool IsOnGroundLevel(int floorGridPosition) => floorGridPosition == MapDescription.GroundIndex;
        public bool IsOnOrAboveGroundLevel(int floorGridPosition) => floorGridPosition <= MapDescription.GroundIndex;

        public GameObject GetPrefabByGridPosition(Vector3Int newGridPosition) =>
            _prefabBuilder.GetPrefabByGridPosition(newGridPosition);

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation positionRotation)
        {
            return _prefabBuilder.GetPrefabConfigurationByTransformData(positionRotation);
        }

        public void ChangePrefabPositionsBy(Vector3 positionChangeDelta) =>
            _prefabBuilder.ChangePrefabPositionsBy(positionChangeDelta);

        public IEnumerable<T> GetPrefabConfigurationsOnWorldPosition<T>(Vector3 transformPosition)
            where T : PrefabConfiguration =>
            _prefabBuilder.GetPrefabConfigurationsOnWorldPosition<T>(transformPosition);

        public GameObject GetPrefabByGuid(string guid) =>
            _prefabBuilder.GetPrefabByGuid(guid);

        public TC GetConfigurationByGuid<TC>(string guid) where TC : PrefabConfiguration =>
            _prefabBuilder.GetConfigurationByGuid<TC>(guid);

        public void RemovePrefab<TC>(TC configuration) where TC : PrefabConfiguration =>
            _prefabBuilder.RemovePrefab(configuration);

        public bool BuildPrefab<TC>(TC configuration, bool isEditorBuild = false) where TC : PrefabConfiguration
            => _prefabBuilder.BuildPrefab(configuration, isEditorBuild);

        public void AddReplacePrefabConfiguration<TC>(TC configuration) where TC : PrefabConfiguration =>
            _prefabBuilder.AddReplacePrefabConfiguration(configuration);

        public IEnumerable<TC> GetConfigurationsByPrefabClass<TC, TP>()
            where TP : PrefabBase where TC : PrefabConfiguration
            => _prefabBuilder.GetConfigurationsByPrefabClass<TC, TP>();

        public IEnumerable<TC> GetConfigurations<TC>(EPrefabType prefabType) where TC : PrefabConfiguration
            => _prefabBuilder.GetConfigurations<TC>(prefabType);

        public bool GetConfigurationByOwnerGuidAndName<TC>(string ownerGuid, string prefabName, out TC configuration)
            where TC : PrefabConfiguration
            => _prefabBuilder.GetConfigurationByOwnerGuidAndName(ownerGuid, prefabName, out configuration);

        public void RemoveConfiguration(string guid) => _prefabBuilder.RemoveConfiguration(guid);

        public void SpawnItem(MapObjectConfiguration configuration) => _itemSpawner.SpawnItem(configuration);

        public List<MapObjectConfiguration> CollectMapObjects() => _itemSpawner.CollectMapObjects();

        public async Task RebuildItems() => await _itemSpawner.RebuildItems();

        public void SetIsCurrentMapOutdoor(bool currentMapIsOutdoor) => IsCurrentMapOutdoor = currentMapIsOutdoor;
    }
}