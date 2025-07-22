using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Flow.UI
{
    public class ItemsUpdater : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private GameObject itemIconPrefab; // Prefab with an Image component

        [Header("Sprite References")]
        [SerializeField] private Sprite[] itemSprites; // Set order to match item IDs

        // Track each icon together with its item ID
        private class ItemEntry
        {
            public int ItemId;
            public GameObject Icon;
        }
        private readonly List<ItemEntry> activeIcons = new();

        public void AddItem(int itemId)
        {
            if (itemId < 0 || itemId >= itemSprites.Length)
            {
                Debug.LogWarning($"Invalid item ID: {itemId}");
                return;
            }

            GameObject icon = Instantiate(itemIconPrefab, container);
            icon.GetComponent<Image>().sprite = itemSprites[itemId];

            activeIcons.Add(new ItemEntry {
                ItemId = itemId,
                Icon = icon
            });
        }

        /// <summary>
        /// Removes the first spawned icon matching this itemId.
        /// </summary>
        public void RemoveItem(int itemId)
        {
            // find the entry
            var entry = activeIcons.Find(e => e.ItemId == itemId);
            if (entry == null)
            {
                Debug.LogWarning($"No active icon for item ID: {itemId}");
                return;
            }

            Destroy(entry.Icon);
            activeIcons.Remove(entry);
        }

        /// <summary>
        /// If you ever need to clear all icons:
        /// </summary>
        public void ClearAll()
        {
            foreach (var e in activeIcons)
                Destroy(e.Icon);
            activeIcons.Clear();
        }
    }
}