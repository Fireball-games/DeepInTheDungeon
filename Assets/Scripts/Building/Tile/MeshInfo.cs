using System;

namespace Scripts.Building.Tile
{
    [Serializable]
    public class MeshInfo : ICloneable
    {
        public string meshName;
        public string materialName;

        public object Clone() => new MeshInfo
        {
            meshName = string.Copy(meshName),
            materialName = string.Copy(materialName)
        };
    }
}