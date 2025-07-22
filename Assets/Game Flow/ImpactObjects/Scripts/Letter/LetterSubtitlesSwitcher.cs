using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Flow.ImpactObjects.Scripts.Letter
{

    [Serializable]
    public class SubtitleSprite
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private float duration;
        
        public Sprite Sprite => sprite;
        public float Duration => duration;
    }
    public class LetterSubtitlesSwitcher: MonoBehaviour
    {
        [SerializeField] private List<SubtitleSprite> frames;

        private Image _image;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        
        private void OnEnable()
        {
            StartCoroutine(PlateSubtitles());
        }

        private IEnumerator PlateSubtitles()
        {
            Debug.Log("PlateSubtitles");
            for (int i = 0; i < frames.Count; i++)
            {
                _image.sprite = frames[i].Sprite;
                yield return new WaitForSeconds(frames[i].Duration);
                var color = _image.color;
                color.a = 255;
                _image.color = color;
            }
        }
    }
}