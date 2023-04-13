using Scripts.Helpers.Extensions;
using Scripts.System;
using Scripts.System.MonoBases;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.Player
{
    public class Grabber : MonoBase
    {
        [Tooltip("Size of image when item is dragged out from inventory")] [SerializeField]
        private Vector2 dragSize = new(100, 100);

        [SerializeField] private Vector3 draggedObjectOffset = new(0, 0, 0.7f);

        [SerializeField] private float maxDragHeight = 0.2f;
        [SerializeField] private float minDragHeight = -0.4f;
        [SerializeField] private float maxDragWidth = 0.4f;
        [SerializeField] private float minDragWidth = -0.4f;
        [SerializeField] private float throwPower = 7f;

        public static Vector2 DragSize { get; private set; }

        private static Rigidbody _rigidBody;
        private static Rigidbody _originalRigidBody;
        private static RigidBodyData _storedRbData;
        private static bool _isGrabbing;
        private static GameObject _grabbedGo;

        private static PlayerController Player => PlayerController.Instance;
        // private static float MaxDragHeight => DragHelper.MaxDragHeight;
        // private static float MinDragHeight => DragHelper.MinDragHeight;
        // private static float MaxDragWidth => DragHelper.MaxDragWidth;
        // private static float MinDragWidth => DragHelper.MinDragWidth;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();

            DragSize = dragSize;
        }

        private void Update()
        {
            if (_isGrabbing)
            {
                SetToMousePosition();
            }
        }

        public static void Grab(GameObject grabbedGo)
        {
            _isGrabbing = false;

            grabbedGo.TryGetComponent(out _originalRigidBody, true);
            SetRigidBody(_originalRigidBody);

            _isGrabbing = true;
        }

        public static void Release()
        {
            _isGrabbing = false;
        }

        private void SetToMousePosition()
        {
            Vector3 mouseScreenPosition = GetMouseScreenPosition();
            // Logger.Log($"mouseScreenPosition: {mouseScreenPosition.ToString().WrapInColor(Color.cyan)}");
            Vector3 playerPosition = Player.transform.localPosition;
            mouseScreenPosition =
                mouseScreenPosition.SetY(Mathf.Clamp(mouseScreenPosition.y, playerPosition.y + minDragHeight, playerPosition.y + maxDragHeight));
            // mouseScreenPosition = mouseScreenPosition.SetZ(Mathf.Clamp(mouseScreenPosition.z, playerPosition.z + MinDragWidth, playerPosition.z + MaxDragWidth));
            Logger.Log($"mouseScreenPosition clamped: {mouseScreenPosition.ToString().WrapInColor(Color.yellow)}");
            // Logger.Log($"object height: {mouseScreenPosition.y.ToString().WrapInColor(Color.cyan)}");
            // targetTransform.position = GetMouseScreenPosition();
            position = mouseScreenPosition;
            transform.localRotation = Quaternion.identity;
        }

        private static void SetRigidBody(Rigidbody rigidBody)
        {
            _storedRbData = new RigidBodyData(rigidBody);
            
            rigidBody.mass = _rigidBody.mass;
            rigidBody.angularDrag = _rigidBody.angularDrag;
            rigidBody.drag = _rigidBody.drag;
            rigidBody.useGravity = _rigidBody.useGravity;
            rigidBody.constraints = _rigidBody.constraints;
        }

        /// <summary>
        /// Gets position of dragged object relative to mouse position relative to the camera.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetMouseScreenPosition()
        {
            return CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition + draggedObjectOffset);
        }

        private struct RigidBodyData
        {
            public float Mass;
            public float AngularDrag;
            public float Drag;
            public bool UseGravity;
            public RigidbodyConstraints Constraints;

            public RigidBodyData(Rigidbody rigidBody)
            {
                Mass = rigidBody.mass;
                AngularDrag = rigidBody.angularDrag;
                Drag = rigidBody.drag;
                UseGravity = rigidBody.useGravity;
                Constraints = rigidBody.constraints;
            }
        }
    }
}