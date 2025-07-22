using Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using Game_Flow.DotVisual.Scripts;
using Game_Flow.DotVisual.Scripts.States;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Game_Flow.ParticleSystem;
using Game_Flow.Player.Scripts;
using WaitForSeconds = UnityEngine.WaitForSeconds;

namespace Game_Flow.Camera
{
    public enum ViewMode { FirstPerson, TopDown }
    public class CameraSwitcher : MonoBehaviour
    {
        private static readonly int Activate = Animator.StringToHash("activate");
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private CinemachineCamera topDownCamera;
        [SerializeField] private CinemachineCamera paintingCamera;
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private GameObject ceiling;
        [SerializeField] private UnityEngine.ParticleSystem p1;
        [SerializeField] private UnityEngine.ParticleSystem p2;
        [SerializeField] private UnityEngine.ParticleSystem p3;
        [SerializeField] private UnityEngine.ParticleSystem p4;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ObjectController objectController;
        [SerializeField] private Animator animator;
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip cameraSwitchAudioClip;
        [SerializeField] private AudioClip dollPlacedAudioClip;
        
        private OpenCloseObjectAudio _objectAudio;
        
        private InputSystem_Actions _inputActions;
        
        private bool _isTopDown = false;
        private bool _canSwitchView = false;
        private ViewMode _currentViewMode = ViewMode.FirstPerson;
        private CinemachineBrain _cinemachineBrain;

        private void Awake()
        {
            _cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            _objectAudio = new OpenCloseObjectAudio(audioSource, dollPlacedAudioClip);
            _objectAudio.SetVolume(0.35f);
        }

        private void OnEnable()
        {
            EventManager.OnDollPlaced += ToggleViewByDoll;
            EventManager.OnPlayerZoneChanged += HandleZoneChange;
            EventManager.OnShowPainting += SwitchToPaintingCamera;
            EventManager.OnExitPainting += ExitPaintingCamera;
        }

        private void OnDisable()
        {
            EventManager.OnPlayerZoneChanged -= HandleZoneChange;
            EventManager.OnDollPlaced -= ToggleViewByDoll;
            EventManager.OnShowPainting -= SwitchToPaintingCamera;
            EventManager.OnExitPainting -= ExitPaintingCamera;
        }

        private void ToggleViewByDoll()
        {
            DollParticles.OnDollPlaced(p1, p2, p3, p4);
            StartCoroutine(DelayedToggleView());
        }

        private IEnumerator DelayedToggleView()
        {
            _objectAudio.PlaySound();
            yield return new WaitForSeconds(3f);
            _objectAudio.SetClip(cameraSwitchAudioClip);
            _isTopDown = !_isTopDown;
            firstPersonCamera.gameObject.SetActive(!_isTopDown);
            topDownCamera.gameObject.SetActive(_isTopDown);
            ChangeViewMode();
            EventManager.ViewModeChanged(_currentViewMode);
        }

        private void ChangeViewMode()
        {
            if (_isTopDown)
            {
                _currentViewMode = ViewMode.TopDown;
                StartCoroutine(SwitchToOrtho());
                _objectAudio.PlaySound();
            }
            else
            {
                StartCoroutine(SwitchToPerspective());
            }
        }
        
        private IEnumerator SwitchToOrtho()
        {
            playerController.IsMovementLocked = true;
            animator.gameObject.SetActive(true);
            animator.SetTrigger(Activate);
            yield return new WaitForSeconds(_cinemachineBrain.DefaultBlend.BlendTime * .18f);
            ceiling.gameObject.SetActive(false);
            objectController.ChangeState(new TopDownState());
            
        }
        
        private IEnumerator SwitchToPerspective()
        {
            mainCamera.fieldOfView = 60f;
            objectController.ChangeState(new FPState());
            yield return new WaitForSeconds(_cinemachineBrain.DefaultBlend.BlendTime * .82f);
            ceiling.gameObject.SetActive(true);
        }

        private void SwitchToPaintingCamera()
        {
            paintingCamera.gameObject.SetActive(true);
        }
        
        private void ExitPaintingCamera()
        {
            paintingCamera.gameObject.SetActive(false);
        }
        
        private void HandleZoneChange(bool canSwitch)
        {
            _canSwitchView = canSwitch;
            if (!_canSwitchView && _isTopDown)
            {
                _isTopDown = false;
                firstPersonCamera.gameObject.SetActive(true);
                topDownCamera.gameObject.SetActive(false);
                _currentViewMode = ViewMode.FirstPerson;
            }
        }
    }
}