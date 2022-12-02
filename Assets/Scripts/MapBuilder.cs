using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class MapBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject ceilingPrefab;
        [SerializeField] private GameObject wallPrefab;

        public event Action OnLayoutBuilt;

        private Transform _layoutParent;

        private void Awake()
        {
            _layoutParent ??= new GameObject("Layout").transform;
        }

        public void BuildMap(MapDescription mapDescription)
        {
            StartCoroutine(BuildLayout(mapDescription.Layout));
        }

        private IEnumerator BuildLayout(List<List<int>> layout)
        {
            for (int x = 0; x < layout.Count; x++)
            {
                for (int y = 0; y < layout[0].Count; y++)
                {
                    if (layout[x][y] == 0) continue;

                    if (layout[x][y] == 1)
                    {
                        if (GameController.GameMode is GameController.EGameMode.Play)
                        {
                            BuildBaseTile(layout, x, y);
                        }
                        
                        yield return null;
                    }
                }
            }

            OnLayoutBuilt?.Invoke();
        }

        private void BuildBaseTile(List<List<int>> layout, int x, int y)
        {
            GameObject tileParent = new($"Tile: [{x}] [{y}]")
            {
                transform =
                {
                    parent = _layoutParent
                }
            };

            Vector3 tilePosition = new(x, 0f, y);

            //floor
            GameObject floor = Instantiate(floorPrefab, tilePosition, Quaternion.identity);
            floor.transform.parent = tileParent.transform;

            //ceiling
            GameObject ceiling = Instantiate(ceilingPrefab, tilePosition, Quaternion.identity);
            ceiling.transform.parent = tileParent.transform;

            //North Wall
            if (layout[x][y + 1] == 0)
            {
                GameObject northWall = Instantiate(wallPrefab, tilePosition, Quaternion.identity);
                northWall.transform.parent = tileParent.transform;
            }

            //East Wall
            if (layout[x + 1][y] == 0)
            {
                GameObject eastWall = Instantiate(wallPrefab, tilePosition, Quaternion.Euler(0f, 90f, 0f));
                eastWall.transform.parent = tileParent.transform;
            }

            //South Wall
            if (layout[x][y - 1] == 0)
            {
                GameObject southWall = Instantiate(wallPrefab, tilePosition, Quaternion.Euler(0f, 180f, 0f));
                southWall.transform.parent = tileParent.transform;
            }

            //West Wall
            if (layout[x - 1][y] == 0)
            {
                GameObject westWall = Instantiate(wallPrefab, tilePosition, Quaternion.Euler(0f, 270f, 0f));
                westWall.transform.parent = tileParent.transform;
            }
        }
    }
}