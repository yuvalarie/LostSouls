using Core.Audio;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.Audio
{
    public class MoovingObjectAudio : ObjectAudio
    {
        public MoovingObjectAudio(AudioSource objectAudioSource, AudioClip draggingSound) : base(objectAudioSource,
            draggingSound) { }
    }
}