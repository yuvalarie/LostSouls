using System;
using System.Collections;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Game_Flow.ImpactObjects.Scripts.Types;
using Game_Flow.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Game_Flow.CollectableObjects
{
    public class CollectableDollObject:MonoBehaviour
    {
        [SerializeField] private OpenCloseImpactObject showcaseDoor1;
        [SerializeField] private OpenCloseImpactObject showcaseDoor2;
        [SerializeField] private ItemsUpdater itemsUpdater;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;
        
        private OpenCloseObjectAudio _objectAudio;

        private void Start()
        {
            if (audioSource != null && audioClip != null)
            {
                _objectAudio = new OpenCloseObjectAudio(audioSource, audioClip);
                _objectAudio.SetVolume(0.5f);
            }
        }

        public void OnCollect()
        {
            if (showcaseDoor1.IsLocked || showcaseDoor2.IsLocked)
            {
                return;
            }
            itemsUpdater.AddItem(1);
            _objectAudio?.PlaySound();
            StartCoroutine(DelayDestroy());
        }
        
        private IEnumerator DelayDestroy()
        {
            GetComponent<Collider>().enabled = false;
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
            foreach (Transform child in GetComponentInChildren<Transform>())
            {
                child.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }    }
}