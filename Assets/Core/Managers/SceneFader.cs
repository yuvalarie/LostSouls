using System.Collections;
using Core.Managers.Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Managers
{
    public class SceneFader : MonoSingleton<SceneFader>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 1f;

        

        public void FadeToScene(string sceneName)
        {
            Debug.Log($"Fading to scene: {sceneName}");
            StartCoroutine(FadeOutIn(sceneName));
        }

        private IEnumerator FadeOutIn(string sceneName)
        {
            yield return StartCoroutine(Fade(0f, 1f)); // Fade to black
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return StartCoroutine(Fade(1f, 0f)); // Fade back to clear
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            float time = 0f;
            canvasGroup.blocksRaycasts = true;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = endAlpha;
            canvasGroup.blocksRaycasts = endAlpha > 0.01f;
        }
    }
}