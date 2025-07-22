using System.Collections;
using DG.Tweening;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using Unity.VisualScripting;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class OpenCloseImpactObject : MonoBehaviour
    {
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float strength = 0.2f;
        [SerializeField] private int vibrato = 10;
        [SerializeField] private bool isShowcase;
        [SerializeField] private bool isLetter;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color highlightColor;
        [SerializeField] private float width;
        [SerializeField] private float scale;
        [SerializeField] private OpenCloseImpactObject otherLetter;
        
        [Header("Audio")]
        [SerializeField] private AudioSource objectAudioSource;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioClip lockedAudioClip;
        
        private bool _isOpen;
        private Animator _animator;
        private OpenCloseObjectAudio _objectAudio = null;
        
        public bool IsLocked { get; set; }
        
        public bool IsLetter => isLetter;
        
        public bool IsOpen => _isOpen;
        
        public bool PlayAudio { get; set; } = true;

        void Start()
        {
            _isOpen = false;
            _animator = gameObject.GetComponent<Animator>();
            
            if (objectAudioSource != null && audioClip != null)
            {
                _objectAudio = new OpenCloseObjectAudio(objectAudioSource, audioClip);
            }
            
            if (isShowcase)
            {
                IsLocked = true;
                _objectAudio.SetClip(lockedAudioClip);
                _objectAudio.SetVolume(0.1f);
            }
            else
            {
                IsLocked = false;
            }
            
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = false;
            }
        }

        public virtual void OpenImpactObject()
        {
            _isOpen = true;
            if (_animator == null) _animator = gameObject.GetComponent<Animator>();
            _animator.SetTrigger("IsOpen");
            Debug.Log("OpenImpactObject");
            if (isShowcase)
            {
                _objectAudio.SetClip(audioClip);
                _objectAudio.SetVolume(1f);
            }
            _objectAudio?.PlaySound();
            if (isLetter && otherLetter != null)
            {
                otherLetter.UnHighlightObject();
                UnHighlightObject();
            }
        }
        
        public void CloseImpactObject()
        {
            _isOpen = false;
            _animator.SetTrigger("IsClose");
            Debug.Log("CloseImpactObject");
            if (PlayAudio)
            {
                _objectAudio?.PlaySound();
            }
            if (isLetter && otherLetter != null)
            {
                otherLetter.HighlightObject();
                HighlightObject();
            }
        }
        
        public void PlayLockedAnimation()
        {
            transform.DOShakePosition(duration, strength, vibrato, 90, false, true);
            StartCoroutine(PlayLockedSound());
        }
        
        private IEnumerator PlayLockedSound()
        {
            _objectAudio?.PlaySound();
            yield return new WaitForSeconds(0.5f);
            _objectAudio?.StopSound();
        }

        public void HighlightObject()
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = true;
                var material = renderer.material;
                if (material == null) continue;
                material.color = highlightColor;
            }
            
        }
        
        public void UnHighlightObject()
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = false;
            }
        }

        public void StopSound()
        {
            _objectAudio?.StopSound();
        }

        public void PlaySound()
        {
            _objectAudio?.PlaySound();
        }
    }
}