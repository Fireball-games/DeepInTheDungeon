using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Building.PrefabsSpawning.Configurations;
using Scripts.Building.Tile;
using Scripts.Helpers;
using Scripts.Helpers.Extensions;
using Scripts.MapEditor;
using Scripts.System;
using Scripts.System.Pooling;
using UnityEngine;
using LayoutType = System.Collections.Generic.List<System.Collections.Generic.List<Scripts.Building.Tile.TileDescription>>;
using Logger = Scripts.Helpers.Logger;

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

        private MapEditorManager EditorManager => MapEditorManager.Instance;
        private GameObject _prefabsParent;
        private HashSet<GameObject> _prefabs;

        private void Awake()
        {
            PhysicalTiles = new Dictionary<Vector3Int, GameObject>();
            NullTilesMap = new Dictionary<int, List<NullTile>>();
            _prefabs = new HashSet<GameObject>();

            if (!LayoutParent)
            {
                LayoutParent = new GameObject("Layout").transform;
                LayoutParent.transform.parent = levelPartsParent.transform;

                _prefabsParent = new GameObject("Prefabs")
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
            StartCoroutine(BuildPrefabsCoroutine(mapDescription.PrefabConfigurations));
        }

        public void SetLayout(TileDescription[,,] layout) => Layout = layout;

        public void DemolishMap()
        {
            foreach (GameObject tile in PhysicalTiles.Values)
            {
                ObjectPool.Instance.ReturnToPool(tile);
            }

            foreach (GameObject prefab in _prefabs)
            {
                Transform offsetTransform = prefab.GetComponentInChildren<MeshFilter>().transform;

                if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

                ObjectPool.Instance.ReturnToPool(prefab);
            }

            NullTilesMap.Clear();
            PhysicalTiles.Clear();
            _prefabs.Clear();
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
        /// Builds new prefab and both stores configuration in MapDescription and GameObject in Prefabs list.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool BuildPrefab(PrefabConfiguration configuration)
        {
            GameObject newPrefab = PrefabStore.Instantiate(configuration.PrefabName, _prefabsParent);

            if (!newPrefab)
            {
                Logger.LogError($"Prefab \"{configuration.PrefabName}\" was not found.");
                return false;
            }

            newPrefab.transform.position = configuration.TransformData.Position;

            if (configuration is TilePrefabConfiguration)
            {
                newPrefab.GetComponentInChildren<MeshFilter>().transform.rotation = configuration.TransformData.Rotation;
            }

            if (configuration is WallConfiguration wallConfiguration)
            {
                newPrefab.transform.localRotation = configuration.TransformData.Rotation;
                
                Transform physicalPart = newPrefab.GetComponentInChildren<MeshFilter>().transform;

                if (physicalPart)
                {
                    Vector3 position = physicalPart.localPosition;
                    position.x += wallConfiguration.Offset;
                    physicalPart.localPosition = position;
                }
            }

            MapDescription.PrefabConfigurations ??= new List<PrefabConfiguration>();

            if (!MapDescription.PrefabConfigurations.Contains(configuration))
            {
                MapDescription.PrefabConfigurations.Add(configuration);
            }

            _prefabs.Add(newPrefab);

            return true;
        }

        public void RemovePrefab(PrefabConfiguration configuration)
        {
            if (configuration == null) return;

            PrefabConfiguration config = MapDescription.PrefabConfigurations.FirstOrDefault(c =>
                c.PrefabName == configuration.PrefabName
                && c.TransformData == configuration.TransformData);

            if (config == null) return;

            MapDescription.PrefabConfigurations.Remove(config);

            GameObject prefabGo = _prefabs.FirstOrDefault(go =>
                go.name == configuration.PrefabName && go.transform.position == configuration.TransformData.Position);

            if (!prefabGo)
            {
                Logger.LogWarning($"No prefab of name \"{configuration.PrefabName}\" found for removal in Prefabs.");
                return;
            }

            _prefabs.Remove(prefabGo);

            if (configuration is TilePrefabConfiguration)
            {
                Layout.ByGridV3Int(prefabGo.transform.position.ToGridPosition()).IsForMovement = true;
            }

            prefabGo.transform.rotation = Quaternion.Euler(Vector3.zero);
            Transform offsetTransform = prefabGo.GetComponentInChildren<MeshFilter>().transform;
            if (offsetTransform) offsetTransform.localPosition = Vector3.zero;

            ObjectPool.Instance.ReturnToPool(prefabGo);
        }

        public GameObject GetPrefabByConfiguration(PrefabConfiguration configuration)
        {
            GameObject result = _prefabs.FirstOrDefault(p => p.transform.position == configuration.TransformData.Position
                                                             && p.name == configuration.PrefabName);

            return !result ? null : result;
        }
        
        public GameObject GetPrefabByGridPosition(Vector3Int position)
        {
            GameObject result = _prefabs.FirstOrDefault(p => p.transform.position == position.ToWorldPositionV3Int());

            return !result ? null : result;
        }

        public void ReplacePrefabConfiguration(PrefabConfiguration newConfiguration)
        {
            int replaceIndex = MapDescription.PrefabConfigurations.FindIndex(c => c.TransformData == newConfiguration.TransformData);
            MapDescription.PrefabConfigurations[replaceIndex] = newConfiguration;
        }

        public void ChangePrefabPositionsBy(Vector3 positionChangeDelta)
        {
            foreach (PrefabConfiguration configuration in MapDescription.PrefabConfigurations)
            {
                configuration.TransformData.Position += positionChangeDelta;
            }
        }

        public PrefabConfiguration GetPrefabConfigurationByTransformData(PositionRotation transformData) =>
            MapDescription.PrefabConfigurations.FirstOrDefault(c => c.TransformData == transformData);

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
            foreach (GameObject prefab in _prefabs)
            {
                prefab.SetActive(floorVisibilityMap[Mathf.RoundToInt(-prefab.transform.position.y)]);
            }
        }

        private IEnumerator BuildLayoutCoroutine(TileDescription[,,] layout)
        {
            Layout = layout;

            _playBuilder = new PlayModeBuilder(this);
            _editorBuilder = new EditorModeBuilder(this);

            for (int floor = 0; floor < layout.GetLength(0); floor++)
            {
                for (int row = 0; row < layout.GetLength(1); row++)
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
                }
            }

            OnLayoutBuilt?.Invoke();
        }

        private IEnumerator BuildPrefabsCoroutine(List<PrefabConfiguration> configurations)
        {
            foreach (PrefabConfiguration configuration in configurations)
            {
                BuildPrefab(configuration);

                yield return null;
            }
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
    }
}