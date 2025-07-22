using Core.Audio;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.Audio
{
    public class OpenCloseObjectAudio : ObjectAudio
    {
        public OpenCloseObjectAudio(AudioSource objectAudioSource, AudioClip sound) : base(objectAudioSource, sound)
        {
            objectAudioSource.loop = false;
        }
        
        public void SetVolume(float volume)
        {
            ObjectAudioSource.volume = volume;
        }
    }
}