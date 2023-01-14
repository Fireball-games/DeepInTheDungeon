﻿namespace Scripts.MapEditor
{
    public class Enums
    {
        public enum EWorkMode
        {
            None = 0,
            Build = 1,
            Select = 2,
            Walls = 3,
            PrefabTiles = 4,
            Prefabs = 5,
            Items = 6,
            Enemies = 7,
            Triggers = 8,
        }

        public enum ELevel
        {
            Equal = 0,
            Upper = 1,
            Lower = 2
        }

        public enum ETriggerEditMode
        {
            None = 0,
            EditTrigger = 1,
            EditReceiver = 2,
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
    }
}