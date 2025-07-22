using System;
using System.Collections;
using Game_Flow.UI;
using UnityEngine;

namespace Game_Flow.CollectableObjects
{
    public class CollectableManDollObject : MonoBehaviour
    {
        [Header("Assign a disabled DollAnimationScript in the scene")]
        [SerializeField] private DollAnimationScript dollAnimationScript;
        [SerializeField] private ItemsUpdater itemsUpdater;

        /// <summary>
        /// Call this when the player collects.
        /// </summary>
        public void OnCollect(Action callback)
        {
            itemsUpdater.ClearAll();
            callback?.Invoke();
            dollAnimationScript.gameObject.SetActive(true);
        }
        
    }
}