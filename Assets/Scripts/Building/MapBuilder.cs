using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Building.Walls;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using static Scripts.Enums;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;

namespace Scripts.Building
{
    public class MapBuilder : MonoBehaviour
    {
        public DefaultBuildPartsProvider defaultsProvider;
        [SerializeField] private GameObject levelPartsParent;

        private TileBuilderBase _playBuilder;
        private TileBuilderBase _editorBuilder;

        public event Action OnLayoutBuilt;

        internal Transform LayoutParent;
        internal TileDescription[,,] Layout;
        internal Dictionary<Vector3Int, GameObject> PhysicalTiles;
        internal Dictionary<int, List<NullTile>> NullTilesMap;
        internal MapDescription MapDescription;
        internal GameObject PrefabsParent;

        private static MapEditorManager EditorManager => MapEditorManager.Instance;
        private PrefabBuilder _prefabBuilder;
        internal HashSet<GameObject> Prefabs;

        private void Awake()
        {
            PhysicalTiles = new Dictionary<Vector3Int, GameObject>();
            NullTilesMap = new Dictionary<int, List<NullTile>>();
            Prefabs = new HashSet<GameObject>();
            _prefabBuilder = new PrefabBuilder();

            if (!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                LayoutParent.transform.parent = levelPartsParent.transform;

                PrefabsParent = new GameObject("Prefabs")
                {
                    transform =
                    {
                        parent = levelPartsParent.transform
                    }
                };
            }
        }

        public void BuildMap(MapDescription mapDescription)
        {
            DemolishMap();

            MapDescription = mapDescription;

            StartCoroutine(BuildLayoutCoroutine(mapDescription.Layout));
            _prefabBuilder.BuildPrefabs(mapDescription.PrefabConfigurations);
        }

        public void SetLayout(TileDescription[,,] layout) => Layout = layout;

        public void DemolishMap()
        {
            foreach (GameObject tile in PhysicalTiles.Values)
            {
                ObjectPool.Instance.ReturnToPool(tile);
            }

            foreach (GameObject prefab in Prefabs)
            {
                Transform offsetTransform = prefab.GetBody();

                if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

                ObjectPool.Instance.ReturnToPool(prefab);
            }

            NullTilesMap.Clear();
            PhysicalTiles.Clear();
            Prefabs.Clear();
        }

        /// <summary>
        /// Build a new tile where was previously null tile. Or null tile where was previously a tile.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="floor"></param>
        public void RebuildTile(int floor, int row, int column)
        {
            if (GameManager.Instance.GameMode is GameManager.EGameMode.Play)
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

        public static MapDescription GenerateDefaultMap(int floors, int rows, int columns)
        {
            TileDescription[,,] layout = new TileDescription[floors, rows, columns];

            Vector3Int center = new(floors / 2, rows / 2, columns / 2);

            layout = AddTilesToCenterOfLayout(layout);

            return new MapDescription
            {
                Layout = layout,
                StartGridPosition = center,
            };
        }

        public GameObject GetPhysicalTileByGridPosition(int floor, int row, int column)
        {
            Vector3Int worldPosition = new(row, -floor, column);

            return PhysicalTiles[worldPosition];
        }

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

            OnLayoutBuilt?.Invoke();
        }

        private IEnumerator BuildFloor(int floor, TileDescription[,,] layout)
        {
            for (int row = 0; row < layout.GetLength(1); row++)
            {
                _runningRowBuilds += 1;
                StartCoroutine(BuildRow(floor, row, layout));
            }

            _runningFloorBuilds -= 1;

            yield return new WaitUntil(() => _runningRowBuilds == 0);
        }

        private int _runningRowBuilds;

        private IEnumerator BuildRow(int floor, int row, TileDescription[,,] layout)
        {
            for (int column = 0; column < layout.GetLength(2); column++)
            {
                if (GameManager.Instance.GameMode is GameManager.EGameMode.Play)
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
                if (Layout[floor + direction.y, row + direction.x, column + direction.z] == null)
                    tileController.ShowWall(TileDirections.WallDirectionByVector[direction]);
                else
                    tileController.HideWall(TileDirections.WallDirectionByVector[direction]);
            }

            if (GameManager.Instance.GameMode == GameManager.EGameMode.Editor)
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

        public GameObject GetPrefabByGridPosition(Vector3Int newGridPosition) => _prefabBuilder.GetPrefabByGridPosition(newGridPosition);

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation positionRotation)
        {
            return _prefabBuilder.GetPrefabConfigurationByTransformData(positionRotation);
        }

        public void ChangePrefabPositionsBy(Vector3 positionChangeDelta) => _prefabBuilder.ChangePrefabPositionsBy(positionChangeDelta);

        public IEnumerable<T> GetPrefabConfigurationsOnWorldPosition<T>(Vector3 transformPosition) where T : PrefabConfiguration =>
            _prefabBuilder.GetPrefabConfigurationsOnWorldPosition<T>(transformPosition);

        public GameObject GetPrefabByGuid(string guid) =>
            _prefabBuilder.GetPrefabByGuid(guid);

        public TC GetConfigurationByGuid<TC>(string guid) where TC : PrefabConfiguration =>
            _prefabBuilder.GetConfigurationByGuid<TC>(guid);

        public void RemovePrefab<TC>(TC configuration) where TC : PrefabConfiguration => _prefabBuilder.RemovePrefab(configuration);

        public bool BuildPrefab<TC>(TC configuration) where TC : PrefabConfiguration => _prefabBuilder.BuildPrefab(configuration);

        public void AddReplacePrefabConfiguration<TC>(TC configuration) where TC : PrefabConfiguration =>
            _prefabBuilder.AddReplacePrefabConfiguration(configuration);

        public IEnumerable<TC> GetConfigurationsByPrefabClass<TC, TP>() where TP : PrefabBase where TC : PrefabConfiguration 
            => _prefabBuilder.GetConfigurationsByPrefabClass<TC, TP>();

        public IEnumerable<TC> GetConfigurations<TC>(EPrefabType prefabType) where TC : PrefabConfiguration 
            => _prefabBuilder.GetConfigurations<TC>(prefabType);
    }
}