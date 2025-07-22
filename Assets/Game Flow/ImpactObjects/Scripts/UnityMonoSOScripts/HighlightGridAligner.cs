using System.Collections.Generic;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    public class HighlightGridAligner : MonoBehaviour
    {
        [Tooltip("Reference to your Grid component")]
        [SerializeField] private Grid grid;

        [Tooltip("Drag in all of your highlight-zone GameObjects here")]
        [SerializeField] private List<GameObject> highlights;

        [Tooltip("If true, will snap on Start(); you can also call SnapHighlights() from the Inspector menu")]
        [SerializeField] private bool snapOnStart = true;

        private void Start()
        {
            if (snapOnStart)
                SnapAndRegister();
        }

        [ContextMenu("Snap And Register Highlights")]
        public void SnapAndRegister()
        {
            if (grid == null)
            {
                Debug.LogWarning("Grid reference is missing!", this);
                return;
            }

            foreach (var go in highlights)
            {
                // Snap highlight to grid center
                var overCell = grid.WorldToCell(go.transform.position);
                var center = grid.GetWorldCenter(overCell);
                center.y = go.transform.position.y;
                go.transform.position = center;

                // Register the GameObject with the grid
                grid.RegisterHighlightZone(go.GetComponent<Renderer>());
            }
        }
    }
}