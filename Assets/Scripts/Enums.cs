﻿namespace Scripts
{
    public class Enums
    {
        public enum EPrefabType
        {
            Invalid = 0,
            Wall = 1,
            WallOnWall = 2,
            WallBetween = 3,
            WallForMovement = 4,
            Enemy = 5,
            Prop = 6,
            Item = 7,
            PrefabTile = 8,
        }
        
        public enum ETriggerType
        {
            Repeat = 1,
            OneOff = 2,
        }

        public enum EMovementType
        {
            None = 0,
            ThereAndBack = 1,
            Switch = 2,
        }
    }
}