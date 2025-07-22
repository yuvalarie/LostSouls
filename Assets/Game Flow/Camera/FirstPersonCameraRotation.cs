using Core.Input_System;
using Game_Flow.Player.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class FirstPersonCameraRotation : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private float sensitivity = 2f;

        private InputSystem_Actions _inputActions;
        private Vector2 _lookInput;
        private float _xRotation = 0f;
        [SerializeField] private PlayerController player;
        
        public InputSystem_Actions InputActions => _inputActions;

        void Awake()
        {
            _inputActions = InputSystemBuffer.Instance.InputSystem;;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void OnEnable()
        {
            _inputActions.Player.Disable();
            _inputActions.Player.Look.performed += OnLookPerformed;
            _inputActions.Player.Look.canceled += OnLookCanceled;
        }

        void OnDisable()
        {
            _inputActions.Player.Look.performed -= OnLookPerformed;
            _inputActions.Player.Look.canceled -= OnLookCanceled;
            _inputActions.Player.Disable();
        }

        void Update()
        {
            float mouseX = _lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = _lookInput.y * sensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if(!player.IsMovementLocked) _lookInput = context.ReadValue<Vector2>();
            else _lookInput = Vector2.zero;
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            _lookInput = Vector2.zero;
        }
    }
}