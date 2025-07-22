using System;
using Core.Audio;
using Game_Flow.ImpactObjects.Scripts.Audio;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game_Flow.ImpactObjects.Scripts.Letter
{
    public class PaintingAudio: MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip paintingAudio;
        
        private ObjectAudio _objectAudio;
        
        private void Awake()
        {
            _objectAudio = new OpenCloseObjectAudio(audioSource, paintingAudio);
            audioSource.volume = .3f;
        }

        private void Play()
        {
            if (audioSource == null || audioSource.isPlaying) return;
            audioSource.Play();
        }

    }
}