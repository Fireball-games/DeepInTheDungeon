using UnityEngine;

namespace Scripts.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInput : MonoBehaviour
    {
        public KeyCode forward = KeyCode.W;
        public KeyCode back = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode turnLeft = KeyCode.Q;
        public KeyCode turnRight = KeyCode.E;

        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(forward)) _playerController.MoveForward();
            if (Input.GetKeyUp(back)) _playerController.MoveBackwards();
            if (Input.GetKeyUp(left)) _playerController.MoveLeft();
            if (Input.GetKeyUp(right)) _playerController.MoveRight();
            if (Input.GetKeyUp(turnLeft)) _playerController.RotateLeft();
            if (Input.GetKeyUp(turnRight)) _playerController.RotateRight();
        }
    }
}