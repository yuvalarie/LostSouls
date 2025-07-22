using Core.Managers;
using Game_Flow.Camera;
using UnityEngine;

namespace Game_Flow.Player.Scripts
{
    public enum WalkingSurface
    {
        ConcreteFloor,
        WoodenFloor,
        WoodenStairs
    }
    
    public class PlayerAudio
    {
        private readonly AudioSource _BGAudioSource;
        private readonly AudioSource _stepsAudioSource;
        private readonly AudioClip _whiteNoise;
        private readonly AudioClip _walkingOnConcreteFloor;
        private readonly AudioClip _walkingOnWoodenFloor;
        private readonly AudioClip _walkingOnWoodenStairs;
        private WalkingSurface _currentSurfaceType;
        
        public PlayerAudio(AudioSource stepsAudioSource, AudioSource bgAudioSource, AudioClip whiteNoise, AudioClip walkingOnConcreteFloor, AudioClip walkingOnWoodenFloor, AudioClip walkingOnWoodenStairs)
        {
            _stepsAudioSource = stepsAudioSource;
            _BGAudioSource = bgAudioSource;
            _walkingOnWoodenFloor = walkingOnWoodenFloor;
            _walkingOnConcreteFloor = walkingOnConcreteFloor;
            _walkingOnWoodenStairs = walkingOnWoodenStairs;
            _whiteNoise = whiteNoise;
            _stepsAudioSource.loop = true;
            _currentSurfaceType = WalkingSurface.ConcreteFloor;

            EventManager.OnViewModeChanged += StopFootstepSound;
        }
        
        public void PlayWhiteNoise()
        {
            _BGAudioSource.clip = _whiteNoise;
            _BGAudioSource.loop = true;
            _BGAudioSource.volume = 0.1f;
            _BGAudioSource.Play();
        }
        
        public void PlayFootstepSound(WalkingSurface surfaceType)
        {
            if (_stepsAudioSource.isPlaying && _currentSurfaceType.Equals(surfaceType))
                return;

            AudioClip clipToPlay;

            switch (surfaceType)
            {
                case WalkingSurface.ConcreteFloor:
                    clipToPlay = _walkingOnConcreteFloor;
                    break;
                case WalkingSurface.WoodenFloor:
                    clipToPlay = _walkingOnWoodenFloor;
                    break;
                case WalkingSurface.WoodenStairs:
                    clipToPlay = _walkingOnWoodenStairs;
                    break;
                default:
                    Debug.LogWarning("Unknown surface type: " + surfaceType);
                    return;
            }

            if (clipToPlay != null)
            {
                _stepsAudioSource.clip = clipToPlay;
                _stepsAudioSource.Play();
            }
            _currentSurfaceType = surfaceType;
        }

        public void StopFootstepSound()
        {
            if (_stepsAudioSource == null)
            {
                Debug.LogWarning("Steps audio source is null.");
                return;
            }
            if (_stepsAudioSource.isPlaying)
            {
                _stepsAudioSource.Stop();
            }
        }

        private void StopFootstepSound(ViewMode mode)
        {
            StopFootstepSound();
        }
        
        public void Cleanup()
        {
            EventManager.OnViewModeChanged -= StopFootstepSound;
        }
    }
}