using Core.Input_System;
using Game_Flow.DotVisual.Scripts;
using Game_Flow.Player.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class HeadBob : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private float bobSpeed = 14f;
        [SerializeField] private float bobAmount = 0.05f;
        [SerializeField] private ObjectController objectController;

        private float _defaultY;
        private float _timer;
        private Vector2 _moveInput;
        private InputSystem_Actions _inputActions;


        void Start()
        {
            _defaultY = transform.localPosition.y;
        }
        
        void Awake()
        {
            _inputActions = InputSystemBuffer.Instance.InputSystem;;
        }
        
        void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
        }

        void OnDisable()
        {
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Disable();
        }
        
        void Update()
        {
            if (objectController.IsLocked)
                return;
            
            bool isMoving = _moveInput.sqrMagnitude > 0.01f;
            bool isGrounded = controller.IsGrounded;

            if (isMoving && isGrounded)
            {
                _timer += Time.deltaTime * bobSpeed;
                float newY = _defaultY + Mathf.Sin(_timer) * bobAmount;
                transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            }
            else
            {
                _timer = 0f;
                Vector3 target = new Vector3(transform.localPosition.x, _defaultY, transform.localPosition.z);
                transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * bobSpeed);
            }
        }
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }
    }
}