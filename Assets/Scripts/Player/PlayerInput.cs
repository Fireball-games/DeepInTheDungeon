using UnityEngine;

namespace Scripts.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInput : MonoBehaviour
    {
        public KeyCode forward = KeyCode.W;
        public KeyCode back = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode turnLeft = KeyCode.Q;
        public KeyCode turnRight = KeyCode.E;
        public KeyCode toggleLookingMode = KeyCode.R;

        private PlayerMovement _player;

        private void Awake()
        {
            _player = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(forward)) _player.MoveForward();
            if (Input.GetKeyUp(back)) _player.MoveBackwards();
            if (Input.GetKeyUp(left)) _player.MoveLeft();
            if (Input.GetKeyUp(right)) _player.MoveRight();
            if (Input.GetKeyUp(turnLeft)) _player.RotateLeft();
            if (Input.GetKeyUp(turnRight)) _player.RotateRight();

            if (Input.GetKeyUp(toggleLookingMode)) PlayerCameraController.Instance.IsLookModeOn = !PlayerCameraController.Instance.IsLookModeOn;
        }
    }
}