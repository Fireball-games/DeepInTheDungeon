using Scripts.Helpers.Extensions;
using Scripts.System.Pooling;
using UnityEngine;

namespace Scripts.Building
{
    public class PlayModeBuilder : TileBuilderBase
    {
        public PlayModeBuilder(MapBuilder mapBuilder) : base(mapBuilder)
        {}

        protected override void BuildNullTile(int floor, int row, int column)
        {
            Vector3Int gridPosition = new(floor, row, column);
            
            if (MapBuilder.MapDescription.IsOutdoor
                && MapBuilder.IsEdgeTile(gridPosition)
                && MapBuilder.IsOnGroundLevel(floor))
            {
                GameObject newNullTile = ObjectPool.Instance.Get(DefaultsProvider.defaultNullCubePrefab,
                    gridPosition.ToWorldPositionV3Int(),
                    Quaternion.identity,
                    LayoutParent.gameObject);
                
                NullTile nullTileComponent = newNullTile.GetComponent<NullTile>();
                nullTileComponent.Initialize();
                nullTileComponent.ShowFloorOnly();
                newNullTile.transform.localScale = Vector3.one;
            }
        }
    }
}