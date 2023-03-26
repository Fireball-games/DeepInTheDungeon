using static Scripts.Enums;

namespace Scripts.Helpers
{
    public static class TagsManager
    {
        public static string Get(ETag tag) => tag switch
        {
            ETag.Player => "Player",
            ETag.Wall => "Wall",
            ETag.Transport => "Transport",
            ETag.Hazard => "Hazard",
            ETag.PickupCollider => "PickupCollider",
            _ => null
        };
    }
}