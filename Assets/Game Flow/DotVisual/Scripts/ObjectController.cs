using System;
using System.Collections.Generic;
using Core.Attributes;
using Core.Input_System;
using Core.Managers;
using Game_Flow.DotVisual.Scripts.States;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.DotVisual.Scripts
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class ObjectController : MonoBehaviour
    {
        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private List<MonoImpactObject> impactObjects;
        [SerializeField,ReadOnly] private MonoImpactObject currentImpactObject;
        [SerializeField] private Grid grid;
        [SerializeField] private BoxCombinationHandler boxCombinationHandler;
        [SerializeField] private List<GameObject> lightsToDisableInTopDownView;
        [SerializeField] private List<GameObject> lightsToEnableInTopDownView;
        
        public List<GameObject> LightsToDisableInTopDownView => lightsToDisableInTopDownView;
        public List<GameObject> LightsToEnableInTopDownView => lightsToEnableInTopDownView;
        private IObjeckLockingState _currentState;
        private Transform _origin;
        private InputSystem_Actions _inputSystemActions;
        private Vector2 _input;
        private MonoImpactObject _target;
        private bool _isLocked;
        public bool IsLocked { get => _isLocked; private set => _isLocked = value;}
        public Grid Grid => grid;
        public BoxCombinationHandler BoxCombinationHandler => boxCombinationHandler;

        public MonoImpactObject CurrentImpactObject => currentImpactObject;

        private void Awake()
        {
            if (dotPrefab != null)
            {
                dotPrefab = Instantiate(dotPrefab);
            }
            _currentState = new FPState();
            _origin = GetComponent<UnityEngine.Camera>().transform;
            _currentState.EnterState(_origin, dotPrefab, impactObjects, this);
            _inputSystemActions = InputSystemBuffer.Instance.InputSystem;;
        }

        private void OnEnable()
        {
            _inputSystemActions.Enable();
            _inputSystemActions.Player.Enable();
            _inputSystemActions.Player.Lock.performed += OnLock;
            _inputSystemActions.Player.Lock.canceled += OnUnlock;
            _inputSystemActions.Player.Move.performed += OnMovePerformed;
            _inputSystemActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            _inputSystemActions.Player.Lock.performed -= OnLock;
            _inputSystemActions.Player.Lock.canceled -= OnUnlock;
            _inputSystemActions.Player.Move.performed -= OnMovePerformed;
            _inputSystemActions.Player.Move.canceled -= OnMoveCanceled;
            _inputSystemActions.Player.Disable();
            _inputSystemActions.Disable();
        }

        private void Update()
        {
            _currentState?.Update();
            _currentState?.GetTarget(out _target);
            currentImpactObject = _target;
            if (IsLocked && !BoxCombinationHandler.GameFinished)
            {
                Vector3 targetMovement = _currentState.CalculateMovement(_input);
                _target?.UpdateObject(targetMovement);
                Debug.Log(_input);
            }
            else if(_currentState is TopDownState && !_target.IsMoving)
            {
                _target?.StopAudio();
            }
        }
        
        public void ChangeState(IObjeckLockingState newState)
        {
            if (_currentState != null && _currentState.GetType() != newState.GetType())
            {
                _currentState.ExitState();
                _currentState = newState;
                _currentState.EnterState(_origin, dotPrefab,impactObjects,this);
            }
        }

        private void OnLock(InputAction.CallbackContext context)
        {
           
            if (_target != null)
            {
                _target.Activate();
                IsLocked = true;
                EventManager.StartRumble(.2f, .05f, .1f);
            }
        }
        
        private void OnUnlock(InputAction.CallbackContext context)
        {
            
            if (_target != null)
            {
                _target.DeActivate();
                IsLocked = false;
            }
        }
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _input = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _input = Vector2.zero;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // only when playing
            if (!Application.isPlaying || _currentState is not TopDownState topState) 
                return;

            // origin point (camera pivot)
            if (_target == null) 
                return;

            Vector3 origin = _target.transform.position;

            // draw the input direction in yellow
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + topState.DebugInputDir * 2f);

            // if there’s a next target, draw a green sphere on it
            if (topState.DebugNextTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(topState.DebugNextTarget.transform.position, 0.5f);
            }
        }
#endif


    }
}