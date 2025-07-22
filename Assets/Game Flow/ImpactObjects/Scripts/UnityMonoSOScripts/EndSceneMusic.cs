using Core.Audio;
using Core.Managers;
using Core.Managers.Core.Managers;
using Game_Flow.ImpactObjects.Scripts.Audio;
using Unity.VisualScripting;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    public class EndSceneMusic: MonoSingleton<EndSceneMusic>
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;
        private MoovingObjectAudio _objectAudio;

        protected override void Awake()
        {
            base.Awake(); 
            _objectAudio = new MoovingObjectAudio(audioSource, audioClip);
        }


        public void PlayMusic()
        {
            _objectAudio.PlaySound();
        }

        public void StopMusic()
        {
            if (_objectAudio != null)
            {
                _objectAudio.StopSound();
            }
        }
        
    }
}