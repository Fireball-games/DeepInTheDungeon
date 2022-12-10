namespace Scripts.MapEditor
{
    public class Enums
    {
        public enum EWorkMode
        {
            None = 0,
            Build = 1,
            Select = 2,
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
        }
    }
}