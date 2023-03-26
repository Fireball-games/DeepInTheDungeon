using Scripts.System;
using Scripts.System.Saving;
using UnityEngine;

namespace Scripts.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInput : MonoBehaviour
    {
        public KeyCode forward = KeyCode.W;
        public KeyCode back = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode quickLoad = KeyCode.F9;
        public KeyCode quickSave = KeyCode.F5;
        public KeyCode right = KeyCode.D;
        public KeyCode turnLeft = KeyCode.Q;
        public KeyCode turnRight = KeyCode.E;
        public KeyCode toggleLookingMode = KeyCode.R;
        public KeyCode toggleLeaningMode = KeyCode.LeftShift;
        public KeyCode leanForward = KeyCode.W;
        public KeyCode leanLeft = KeyCode.A;
        public KeyCode leanRight = KeyCode.D;
        public KeyCode inventory = KeyCode.I;

        private PlayerMovement _playerMovement;
        private bool _lookModeOnBeforeLean;

        private static PlayerCameraController PlayerCamera => PlayerCameraController.Instance;
        private static PlayerController Player => PlayerController.Instance;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(quickSave))
            {
                SaveManager.QuickSave();
                return;
            }
            if (Input.GetKeyUp(quickLoad))
            {
                GameManager.Instance.QuickLoad();
                return;
            }
            
            if (!PlayerCamera || !GameManager.Instance.MovementEnabled) return;
            
            PlayerCamera.isLeaning = Input.GetKey(toggleLeaningMode);

            if (!PlayerCamera.isLeaning)
            {
                if (Input.GetKeyUp(forward)) _playerMovement.MoveForward();
                else if (Input.GetKeyUp(back)) _playerMovement.MoveBackwards();
                else if (Input.GetKeyUp(left)) _playerMovement.MoveLeft();
                else if (Input.GetKeyUp(right)) _playerMovement.MoveRight();
                else if (Input.GetKeyUp(turnLeft)) _playerMovement.RotateLeft();
                else if (Input.GetKeyUp(turnRight)) _playerMovement.RotateRight();
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
            
            if (Input.GetKeyUp(inventory))
            {
                Player.InventoryManager.Inventory.ToggleInventory();
            }
        }
    }
}