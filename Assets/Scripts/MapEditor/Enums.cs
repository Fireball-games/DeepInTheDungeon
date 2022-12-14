namespace Scripts.MapEditor
{
    public class Enums
    {
        public enum EWorkMode
        {
            None = 0,
            Build = 1,
            Select = 2,
            Walls = 3,
        }

        public enum ELevel
        {
            Equal = 0,
            Upper = 1,
            Lower = 2
        }

        public enum EGridPositionType
        {
            None = 0,
            NullTile = 1,
            EditableTile = 2,
            NullTileAbove = 3,
            EditableTileAbove = 4,
            NullTileBellow = 5,
            EditableTileBellow = 6,
        }

        public enum EWallType
        {
            Invalid = 0,
            OnWall = 1,
            Between = 2,
        }
    }
}