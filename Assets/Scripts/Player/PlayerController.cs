using Scripts.System.MonoBases;

namespace Scripts.Player
{
    public class PlayerController : SingletonNotPersisting<PlayerController>
    {
        public PlayerMovement PlayerMovement => _playerMovement;
        private PlayerMovement _playerMovement;

        protected override void Awake()
        {
            base.Awake();
            
            _playerMovement = GetComponent<PlayerMovement>();
        }
    }
}