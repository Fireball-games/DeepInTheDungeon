﻿using UnityEngine;

namespace Scripts.Building
{
    public class PlayModeBuilder : TileBuilderBase
    {
        public PlayModeBuilder(MapBuilder mapBuilder)
        {
            LayoutParent = mapBuilder.LayoutParent;
            Layout = mapBuilder.Layout;
            FloorPrefab = mapBuilder.FloorPrefab;
            CeilingPrefab = mapBuilder.CeilingPrefab;
            WallPrefab = mapBuilder.WallPrefab;
        }

        public override void BuildTile(int x, int y)
        {
            if (!Layout[x][y].IsForMovement) return;

            if (Layout[x][y].IsForMovement)
            {
                BuildBaseTile(x, y);
            }
        }

        protected override void BuildBaseTile(int x, int y)
        {
            GameObject tileParent = new($"Tile: [{x}] [{y}]")
            {
                transform =
                {
                    parent = LayoutParent
                }
            };

            Vector3 tilePosition = new(x, 0f, y);

            //floor
            GameObject floor = GameObject.Instantiate(FloorPrefab, tilePosition, Quaternion.identity);
            floor.transform.parent = tileParent.transform;

            //ceiling
            GameObject ceiling = GameObject.Instantiate(CeilingPrefab, tilePosition, Quaternion.identity);
            ceiling.transform.parent = tileParent.transform;

            //North Wall
            if (!Layout[x][y + 1].IsForMovement)
            {
                GameObject northWall = GameObject.Instantiate(WallPrefab, tilePosition, Quaternion.identity);
                northWall.transform.parent = tileParent.transform;
            }

            //East Wall
            if (!Layout[x + 1][y].IsForMovement)
            {
                GameObject eastWall = GameObject.Instantiate(WallPrefab, tilePosition, Quaternion.Euler(0f, 90f, 0f));
                eastWall.transform.parent = tileParent.transform;
            }

            //South Wall
            if (!Layout[x][y - 1].IsForMovement)
            {
                GameObject southWall = GameObject.Instantiate(WallPrefab, tilePosition, Quaternion.Euler(0f, 180f, 0f));
                southWall.transform.parent = tileParent.transform;
            }

            //West Wall
            if (!Layout[x - 1][y].IsForMovement)
            {
                GameObject westWall = GameObject.Instantiate(WallPrefab, tilePosition, Quaternion.Euler(0f, 270f, 0f));
                westWall.transform.parent = tileParent.transform;
            }
        }
    }
}