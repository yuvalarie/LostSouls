using System;
using System.Collections;
using Core.Audio;
using Core.Managers;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Game_Flow.ImpactObjects.Scripts.Types;
using Game_Flow.UI;
using UnityEngine;

namespace Game_Flow.CollectableObjects
{
    public class CollectableKeyObject : MonoBehaviour
    {
        
        [SerializeField] private OpenCloseImpactObject showcaseDoor1;
        [SerializeField] private OpenCloseImpactObject showcaseDoor2;
        [SerializeField] private ItemsUpdater itemsUpdater;
        [SerializeField] private GameObject lightSwitch;
        
        [Header("Outline")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color highlightColor;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;

        [SerializeField] private AudioSource bellSource;
        
        private OpenCloseObjectAudio _objectAudio;
        private bool _isLetterOpen = false;
        public bool IsLetterOpen
        {
            get => _isLetterOpen;
            set => _isLetterOpen = value;
        }

        private void Start()
        {
            _objectAudio = new OpenCloseObjectAudio(audioSource, audioClip);
            bellSource.volume = .25f;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = false;
            }
        }

        public void OnCollect()
        {
            if (!lightSwitch.gameObject.activeSelf || !IsLetterOpen)
            {
                EventManager.StartRumble(.5f, 0.2f, .5f);
                return;
            }

            showcaseDoor1.IsLocked = false;
            showcaseDoor2.IsLocked = false;
            itemsUpdater.AddItem(2);
            bellSource.Stop();
            _objectAudio.PlaySound();
            StartCoroutine(DelayDestroy());
        }
        
        private IEnumerator DelayDestroy()
        {
            GetComponent<Collider>().enabled = false;
            GetComponent<Renderer>().enabled = false;
            foreach (Transform child in GetComponentInChildren<Transform>())
            {
                child.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
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
    }
}