using System;

namespace Scripts.Building.Tile
{
    public class WallDescription : ICloneable
    {
        public MeshInfo MeshInfo;
        
        public object Clone() => new WallDescription
        {
            MeshInfo = (MeshInfo) MeshInfo?.Clone()
        };
    }
}