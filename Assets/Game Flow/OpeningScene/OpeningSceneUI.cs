using System.Collections;
using Core.Input_System;
using Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game_Flow.OpeningScene
{
    public class OpeningSceneUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup parentCanvasGroup;
        [SerializeField] private CanvasGroup childCanvasGroup;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = .5f;
        [SerializeField] private float delayBeforeFirstFade = .5f;
        [SerializeField] private float delayBetweenFades = 0.5f;

        private InputSystem_Actions _inputSystem;
        private bool _viewed = false;
        
        private void Start()
        {
            parentCanvasGroup.gameObject.SetActive(true);
            parentCanvasGroup.alpha = 0;
            childCanvasGroup.alpha = 0;
            _inputSystem = InputSystemBuffer.Instance.InputSystem;
        }

        private void OnEnable()
        {
            EventManager.EnterRoom += DisableCanvas;
            _inputSystem = InputSystemBuffer.Instance.InputSystem;
            _inputSystem.OpeningScene.Enable();
            _inputSystem.OpeningScene.All.performed += FadeIn;
            _inputSystem.OpeningScene.Open.performed += OnOpenDoor;
        }


        private void OnDisable()
        {
            EventManager.EnterRoom -= DisableCanvas;
            _inputSystem.OpeningScene.All.performed -= FadeIn;
            _inputSystem.OpeningScene.Open.performed -= OnOpenDoor;
            _inputSystem.OpeningScene.Disable();
        }

        private void FadeIn(InputAction.CallbackContext context)
        {
            if (!_viewed)
            {
                _viewed = true;
                StartCoroutine(FadeInSequence());
            }
        }

        private void OnOpenDoor(InputAction.CallbackContext context)
        {
            _viewed = true;
        }

        private IEnumerator FadeInSequence()
        {
            yield return new WaitForSeconds(delayBeforeFirstFade);
            yield return StartCoroutine(FadeCanvasGroup(parentCanvasGroup, 0f, 1f, fadeInDuration));
            yield return new WaitForSeconds(delayBetweenFades);
            yield return StartCoroutine(FadeCanvasGroup(childCanvasGroup, 0f, 1f, fadeInDuration));
        }

        private IEnumerator FadeOutSequence()
        {
            yield return StartCoroutine(FadeCanvasGroup(childCanvasGroup, childCanvasGroup.alpha, 0f, fadeOutDuration));
            yield return new WaitForSeconds(delayBetweenFades);
            yield return StartCoroutine(FadeCanvasGroup(parentCanvasGroup, parentCanvasGroup.alpha, 0f, fadeOutDuration));
            // parentCanvasGroup.gameObject.SetActive(false);
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cg.alpha = to;
        }

        private void DisableCanvas()
        {
            StartCoroutine(FadeOutSequence());
        }
    }
}