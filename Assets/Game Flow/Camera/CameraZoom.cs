using Core.Input_System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// allows player to zoom in the FOV when holding a button down
namespace Game_Flow.Camera
{
	[RequireComponent (typeof (UnityEngine.Camera))]
	public class CameraZoom : MonoBehaviour
	{
		[SerializeField] private float zoomFOV = 30.0f;
		[SerializeField] private float zoomSpeed = 9f;
		[FormerlySerializedAs("_camera")] [SerializeField] private CinemachineCamera playerCamera;
	
		private float _targetFOV;
		private float _baseFOV;
		private InputSystem_Actions _inputActions;

	
		
		void Awake()
		{
			_inputActions = InputSystemBuffer.Instance.InputSystem;
		}

		void OnEnable()
		{
			_baseFOV = playerCamera.Lens.FieldOfView;
			_targetFOV = _baseFOV;
			_inputActions.Player.Enable();
			_inputActions.Player.Zoom.performed += OnZoomPerformed;
			_inputActions.Player.Zoom.canceled += OnZoomCanceled;
		}

		private void OnZoomPerformed(InputAction.CallbackContext context)
		{
			_targetFOV = zoomFOV;
			Debug.Log("Zoom performed");
		}

		private void OnZoomCanceled(InputAction.CallbackContext context)
		{
			_targetFOV = _baseFOV;
		}
	
		void Update()
		{
			UpdateZoom();
		}
	
		private void UpdateZoom()
		{
			playerCamera.Lens.FieldOfView = Mathf.Lerp(playerCamera.Lens.FieldOfView, _targetFOV, zoomSpeed * Time.deltaTime);
		}

		void OnDisable()
		{
			_inputActions.Player.Zoom.performed -= OnZoomPerformed;
			_inputActions.Player.Zoom.canceled -= OnZoomCanceled;
			_inputActions.Player.Disable();
		}
	}
}
