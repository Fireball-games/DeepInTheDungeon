namespace Scripts.Inventory.Inventories.Items
{
    public class DestroyableProp : MapObject
    {
        public float Health { get; private set; }
        
        public void SetHealth(float health)
        {
            Health = health;
        }
    }
}