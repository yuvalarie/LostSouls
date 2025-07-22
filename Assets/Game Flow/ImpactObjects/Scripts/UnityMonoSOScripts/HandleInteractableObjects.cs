using System;
using Game_Flow.CollectableObjects;
using Game_Flow.ImpactObjects.Scripts.Types;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    public class HandleInteractableObjects : MonoBehaviour
    {
        [SerializeField] private OpenCloseImpactObject[] animatedObjects;
        [SerializeField] private SwitchObject switchObject;
        [SerializeField] private CollectableKeyObject collectableKeyObject;

        public bool InHighlightZone { get; private set; }
        public bool InLetterZone { get; private set; }

        public void CloseAllOpenedObjects()
        {
            foreach (var animatedObject in animatedObjects)
            {
                var openable = animatedObject.GetComponentInChildren<OpenCloseImpactObject>();
                if (openable != null && openable.IsOpen)
                {
                    openable.PlayAudio = false;
                    openable.CloseImpactObject();
                    openable.UnHighlightObject();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HighlightZone"))
            {
                InHighlightZone = true;
                Debug.Log("HighlightZone");
                foreach (var openable in animatedObjects)
                {
                    if (openable != null && !openable.IsLetter)
                    {
                        Debug.Log(openable.name);
                        openable.HighlightObject();
                    }
                }
                if (collectableKeyObject != null)
                {
                    collectableKeyObject.HighlightObject();
                }
            }
            else if (other.CompareTag("LetterZone"))
            {
                InLetterZone = true;
                Debug.Log("LetterZone");
                foreach (var openable in animatedObjects)
                {
                    if (openable != null && openable.IsLetter)
                    {
                        Debug.Log(openable.name);
                        openable.HighlightObject();
                    }
                }
                if (switchObject != null)
                {
                    switchObject.HighlightObject();
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("HighlightZone"))
            {
                InHighlightZone = false;
                Debug.Log("Exit HighlightZone");
                foreach (var openable in animatedObjects)
                {
                    if (openable != null && !openable.IsLetter)
                    {
                        openable.UnHighlightObject();
                    }
                }
                if (collectableKeyObject != null)
                {
                    collectableKeyObject.UnHighlightObject();
                }
            }
            else if (other.CompareTag("LetterZone"))
            {
                InLetterZone = false;
                Debug.Log("Exit LetterZone");
                foreach (var openable in animatedObjects)
                {
                    if (openable != null && openable.IsLetter)
                    {
                        openable.UnHighlightObject();
                    }
                }
                if (switchObject != null)
                {
                    switchObject.UnHighlightObject();
                }
            }
        }
    }
}