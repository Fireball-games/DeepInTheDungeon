namespace Scripts
{
    public static class Enums
    {
        public enum EMainCampaignName
        {
            MainCampaign = 1,
            Demo = 2,
        }
        
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
            TriggerOnWall = 9,
            TriggerTile = 13, 
            TriggerReceiver = 10,
            /// <summary>
            /// Service prefabs, groups prefabs not really fitting to separate category, like EntryPoint, for example.
            /// </summary>
            Service = 11,
            EntryPoint = 12,
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
            Multiple = 3,
            Periodic = 4,
        }

        public enum EActiveProperty
        {
            None = 0,
            Position = 1,
            Rotation = 2,
        }

        public enum ETriggerActivatedDetail
        {
            None = 0,
            SwitchedOn = 1,
            SwitchedOff = 2,
        }
    }
}