using System.Collections;
using Camera;
using Core.Managers;
using Game_Flow.Player.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpeningScene
{
    public class OpeningSceneController : MonoBehaviour
    {
        private static readonly int Open = Animator.StringToHash("Open");
        private static readonly int Close = Animator.StringToHash("Close");
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 moveTarget;
        [SerializeField] private float moveDuration = 2f;
        [SerializeField] private FirstPersonCameraRotation firstPersonCameraRotation;
        [SerializeField] private PlayerController playerController;
        [Header("Audio")]
        [SerializeField] private AudioSource objectAudioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        
        private bool hasStarted = false;
        public void OnStartPressed()
        {
            if (hasStarted) return;
            hasStarted = true;
            StartCoroutine(OpeningSequence());
        }
        
        private IEnumerator OpeningSequence()
        {
            EventManager.OnEnterRoom();
            doorAnimator.SetTrigger(Open);
            objectAudioSource?.PlayOneShot(openSound);
            yield return new WaitForSeconds(1.5f);
            float elapsed = 0f;
            Vector3 start = player.position;
            Vector3 end = moveTarget;
            while (elapsed < moveDuration)
            {
                player.position = Vector3.Lerp(start, end, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            doorAnimator.SetTrigger(Close);
            objectAudioSource?.PlayOneShot(closeSound);
            playerController.IsMovementLocked = false;
            var actions = playerController.InputActions;
            actions.OpeningScene.Disable();
            actions.Player.Enable();
            var cameraActions = firstPersonCameraRotation.InputActions;
            cameraActions.Player.Enable();
            //objectAudioSource?.Stop();
        }
    }
}