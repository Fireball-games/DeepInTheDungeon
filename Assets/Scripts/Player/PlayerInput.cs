using Scripts.System;
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
        public KeyCode toggleLeaningMode = KeyCode.LeftShift;
        public KeyCode leanForward = KeyCode.W;
        public KeyCode leanLeft = KeyCode.A;
        public KeyCode leanRight = KeyCode.D;

        private PlayerMovement _playerMovement;
        private bool _lookModeOnBeforeLean;

        private static PlayerCameraController PlayerCamera => PlayerCameraController.Instance;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (!PlayerCamera || !GameManager.Instance.MovementEnabled) return;
            
            PlayerCamera.isLeaning = Input.GetKey(toggleLeaningMode);

            if (!PlayerCamera.isLeaning)
            {
                if (Input.GetKeyUp(forward)) _playerMovement.MoveForward();
                if (Input.GetKeyUp(back)) _playerMovement.MoveBackwards();
                if (Input.GetKeyUp(left)) _playerMovement.MoveLeft();
                if (Input.GetKeyUp(right)) _playerMovement.MoveRight();
                if (Input.GetKeyUp(turnLeft)) _playerMovement.RotateLeft();
                if (Input.GetKeyUp(turnRight)) _playerMovement.RotateRight();
            }
            else
            {
                PlayerCamera.Lean(Input.GetKey(leanForward), Input.GetKey(leanLeft), Input.GetKey(leanRight));
            }

            if (Input.GetKeyDown(toggleLeaningMode))
            {
                _lookModeOnBeforeLean = PlayerCamera.IsLookModeOn;
                PlayerCamera.IsLookModeOn = false;
            }

            if (Input.GetKeyUp(toggleLeaningMode))
            {
                PlayerCamera.ResetCamera();
                PlayerCamera.IsLookModeOn = _lookModeOnBeforeLean;
            }

            if (!PlayerCamera.isLeaning && Input.GetKeyUp(toggleLookingMode))
            {
                PlayerCamera.HandleLookModeOnKeyClick();
            }
        }
    }
}