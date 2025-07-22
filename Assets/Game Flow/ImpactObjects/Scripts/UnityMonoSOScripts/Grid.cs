using System.Collections.Generic;
using UnityEngine;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using static Game_Flow.ImpactObjects.Scripts.Decorator_Interface.ImpactObjectTypes;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    [ExecuteAlways]
    public class Grid : MonoBehaviour
    {
        [SerializeField] private int rows = 6;
        [SerializeField] private int cols = 4;
        [SerializeField] private float gizmoRadius = 0.05f;
        [SerializeField] private float gizmoHeightOffset = 0.1f;

        public int Cols => cols;
        public int Rows => rows;

        private List<List<Vector3>> allGridCenters = new();
        private List<List<MonoImpactObject>> occupiedCenters = new();
        private Renderer _renderer;

        public List<List<Vector3>> AllGridCenters => new(allGridCenters);
        
        private Dictionary<(int row, int col), List<Renderer>> highlightMap = new();

        void Awake()
        {
            _renderer = GetComponent<Renderer>();
            CalculateGridCenters();
        }

        private void CalculateGridCenters()
        {
            allGridCenters.Clear();
            occupiedCenters.Clear();

            Bounds bounds = _renderer.bounds;
            Vector3 size = bounds.size;
            Vector3 origin = bounds.min;

            float cellWidth = size.x / cols;
            float cellHeight = size.z / rows;

            for (int row = 0; row < rows; row++)
            {
                var rowCenters = new List<Vector3>();
                var rowOccupied = new List<MonoImpactObject>();

                for (int col = 0; col < cols; col++)
                {
                    float x = origin.x + cellWidth * col + cellWidth / 2f;
                    float z = origin.z + cellHeight * row + cellHeight / 2f;
                    float y = bounds.center.y;
                    rowCenters.Add(new Vector3(x, y, z));
                    rowOccupied.Add(null); // Initialize as empty
                }

                allGridCenters.Add(rowCenters);
                occupiedCenters.Add(rowOccupied);
            }
        }

        public bool IsValidCell(int row, int col)
        {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }

        /// <summary>
        /// Marks the given grid cells as occupied by the given MonoImpactObject and returns the center position of the occupied area.
        /// </summary>
        public Vector3 MarkOccupied(MonoImpactObject impactObject, List<(int row, int col)> gridCells)
        {
            float minX = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxZ = float.MinValue;
            float y = 0f;

            foreach (var (row, col) in gridCells)
            {
                if (IsValidCell(row, col))
                {
                    Vector3 cellCenter = allGridCenters[row][col];
                    occupiedCenters[row][col] = impactObject;
                    minX = Mathf.Min(minX, cellCenter.x);
                    maxX = Mathf.Max(maxX, cellCenter.x);
                    minZ = Mathf.Min(minZ, cellCenter.z);
                    maxZ = Mathf.Max(maxZ, cellCenter.z);
                    y = cellCenter.y;
                }
            }

            return new Vector3((minX + maxX) / 2f, y, (minZ + maxZ) / 2f);
        }

        /// <summary>
        /// Unmarks all cells currently occupied by the given MonoImpactObject.
        /// </summary>
        public void UnmarkOccupied(MonoImpactObject impactObject)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (occupiedCenters[row][col] == impactObject)
                    {
                        occupiedCenters[row][col] = null;
                    }
                }
            }
        }

        public bool IsCellsOccupied(List<(int row, int col)> gridCells)
        {
            foreach (var (row, col) in gridCells)
            {
                if (row < 0 || row >= Rows || col < 0 || col >= Cols)
                    return true; // out of bounds = considered occupied

                if (occupiedCenters[row][col] != null)
                    return true; // occupied by another object
            }

            return false; // all cells free
        }

        void OnDrawGizmos()
        {
            if (allGridCenters == null || allGridCenters.Count == 0)
                CalculateGridCenters();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 center = allGridCenters[row][col];
                    bool isOccupied = occupiedCenters.Count > row && occupiedCenters[row].Count > col && occupiedCenters[row][col] != null;
                    Gizmos.color = isOccupied ? Color.green : Color.red;

                    Vector3 drawPosition = center + Vector3.up * gizmoHeightOffset;
                    Gizmos.DrawSphere(drawPosition, gizmoRadius);
                }
            }
        }
        
        public Vector3 GetWorldCenter((int row, int col) cell)
        {
            cell.row = Mathf.Clamp(cell.row, 0, rows - 1);
            cell.col = Mathf.Clamp(cell.col, 0, cols - 1);
            return allGridCenters[cell.row][cell.col];
        }
        
        public (int row, int col) WorldToCell(Vector3 position)
        {
            Bounds bounds = _renderer.bounds;
            float cellWidth = bounds.size.x / cols;
            float cellHeight = bounds.size.z / rows;

            int col = Mathf.FloorToInt((position.x - bounds.min.x) / cellWidth);
            int row = Mathf.FloorToInt((position.z - bounds.min.z) / cellHeight);
            return (Mathf.Clamp(row, 0, rows - 1), Mathf.Clamp(col, 0, cols - 1));
        }

        public MonoImpactObject GetOccupant(int row, int col)
        {
            if (IsValidCell(row, col))
                return occupiedCenters[row][col];
            return null;
        }
        
        
        /// <summary>
        /// Register a highlight zone GameObject to this grid cell.
        /// </summary>
        public void RegisterHighlightZone(Renderer highlight)
        {
            var cell = WorldToCell(highlight.transform.position);
            if (!highlightMap.ContainsKey(cell))
                highlightMap[cell] = new List<Renderer>();

            highlightMap[cell].Add(highlight);
        }

        /// <summary>
        /// Returns highlight zone GameObjects for the given cells.
        /// </summary>
        public List<Renderer> GetHighlightZones(IEnumerable<(int row, int col)> cells)
        {
            var result = new List<Renderer>();
            foreach (var cell in cells)
            {
                if (highlightMap.TryGetValue(cell, out var list))
                    result.AddRange(list);
            }
            return result;
        }
        
        /// <summary>
        /// Highlights the zones in the given cells with the specified color.
        /// </summary>
        public void HighlightZones(IEnumerable<(int row, int col)> cells, Color color)
        {
            foreach (var go in GetHighlightZones(cells))
            {
                if (go == null) continue;
                var renderers = go.GetComponentsInChildren<Renderer>();
                foreach (var rend in renderers)
                {
                    rend.enabled = true;
                    if (rend.material != null)
                        rend.material.color = color;
                }
            }
        }

        /// <summary>
        /// Unhighlights the zones in the given cells.
        /// </summary>
        public void UnhighlightZones(IEnumerable<(int row, int col)> cells)
        {
            foreach (var go in GetHighlightZones(cells))
            {
                if (go == null) continue;
                var renderers = go.GetComponentsInChildren<Renderer>();
                foreach (var rend in renderers)
                {
                    rend.enabled = false;
                }
            }
        }
        
        

    }
}
