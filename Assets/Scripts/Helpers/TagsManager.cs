using static Scripts.Enums;

namespace Scripts.Helpers
{
    public static class TagsManager
    {
        public const string Player = "Player";
        public const string Wall = "Wall";
        public const string Transport = "Transport";
        public const string Hazard = "Hazard";
        public const string PickupCollider = "PickupCollider";

        public static string Get(ETag tag) => tag switch
        {
            ETag.Player => Player,
            ETag.Wall => Wall,
            ETag.Transport => Transport,
            ETag.Hazard => Hazard,
            ETag.PickupCollider => PickupCollider,
            _ => null
        };

    }
}