using UnityEngine;

namespace Core.Audio
{
    public abstract class ObjectAudio
    {
        protected readonly AudioSource ObjectAudioSource;

        protected ObjectAudio(AudioSource objectAudioSource, AudioClip sound)
        {
            ObjectAudioSource = objectAudioSource;
            ObjectAudioSource.clip = sound;
            ObjectAudioSource.loop = true;
        }
        
        public void PlaySound()
        {
            if (!ObjectAudioSource.isPlaying)
            {
                ObjectAudioSource.Play();
            }
        }
        
        public void StopSound()
        {
            if (ObjectAudioSource.isPlaying)
            {
                ObjectAudioSource.Stop();
            }
        }
        
        public void SetClip(AudioClip sound)
        {
            ObjectAudioSource.clip = sound;
        }
    }
}