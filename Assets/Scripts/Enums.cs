namespace Scripts
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
            Trigger = 9,
            TriggerReceiver = 10,
        }
        
        public enum ETriggerType
        {
            OneOff = 1,
            Repeat = 2,
            Multiple = 3,
        }

        public enum ETriggerMoveType
        {
            None = 0,
            ThereAndBack = 1,
            Switch = 2,
        }

        public enum EActiveProperty
        {
            None = 0,
            Position = 1,
            Rotation = 2,
        }
    }
}