using System;
using System.Collections;
using Core.Audio;
using Core.Managers;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game_Flow.CollectableObjects
{
    public class SwitchObject:MonoBehaviour
    {
        [SerializeField] private GameObject objectToSwitch;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clip;
        [FormerlySerializedAs("renderer")] [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color highlightColor;
        [SerializeField] private Animator animator;
        
        private OpenCloseObjectAudio _objectAudio;
        
        private void Start()
        {
            _objectAudio = new OpenCloseObjectAudio(audioSource, clip);
            _objectAudio.SetVolume(0.3f);
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
        }

        private void OnEnable()
        {
            EventManager.OnDollPlaced += turnOnLightsInTopDownState;
        }

        private void OnDisable()
        {
            EventManager.OnDollPlaced -= turnOnLightsInTopDownState;
        }

        public void turnOnLightsInTopDownState()
        {
            objectToSwitch.SetActive(true);
        }
        public void ControlLights()
        {
            objectToSwitch.SetActive(!objectToSwitch.activeSelf);
            StartCoroutine(PlaySound());
            if (animator == null) return;
            if (objectToSwitch.activeSelf)
            {
                animator.SetTrigger("IsOn");
            }
            else
            {
                animator.SetTrigger("IsOff");
            }
        }

        private IEnumerator PlaySound()
        {
            _objectAudio.PlaySound();
            yield return new WaitForSeconds(.5f);
            _objectAudio.StopSound();
        }

        public void HighlightObject()
        {
            if (renderers == null) return;
            foreach (var rend in renderers)
            {
                if (rend == null) continue;
                rend.enabled = true;
                var material = rend.material;
                if (material == null) continue;
                material.color = highlightColor;
            }
        }
        
        public void UnHighlightObject()
        {
            if (renderers == null) return;
            foreach (var rend in renderers)
            {
                if (rend == null) continue;
                rend.enabled = false;
            }
        }

        public void TurnLightsOn()
        {
            if (objectToSwitch.activeSelf) { return; }
            ControlLights();
        }
    }
}