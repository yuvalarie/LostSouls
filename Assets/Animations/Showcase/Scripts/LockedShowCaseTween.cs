using System;
using DG.Tweening;
using UnityEngine;

namespace Animations.Showcase.Scripts
{
    public class LockedShowCaseTween : MonoBehaviour
    {
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float strength = 0.2f;
        [SerializeField] private int vibrato = 10;
        
        public bool IsLocked { get; private set; } = true;

        public void PlayLockedAnimation()
        {
            transform.DOShakePosition(duration, strength, vibrato, 90, false, true);
        }
    }
}