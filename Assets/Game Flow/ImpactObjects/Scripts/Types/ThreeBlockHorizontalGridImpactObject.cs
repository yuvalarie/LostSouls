using System.Collections.Generic;
using System.Data.SqlTypes;
using DG.Tweening;
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class ThreeBlockHorizontalGridImpactObject : ImpactObjectDecorator
    {
        private readonly Grid  _grid;
        private Tween          _moveTween;

        public ThreeBlockHorizontalGridImpactObject(
            IImpactObject inner,
            MonoImpactObject mono,
            ImpactObjectStats stats,
            Grid grid
        ) : base(inner, mono, stats)
        {
            _grid               = grid;
        }

        public override void UpdateImpact(Vector3 direction)
        {
            base.UpdateImpact(direction);

            // 1) if a move‐tween is playing, do nothing
            if (_moveTween != null && _moveTween.IsActive() && !_moveTween.IsComplete())
                return;

            // 2) clear the old footprint so occupancy checks ignore us
            var oldCells = new List<(int row, int col)>(Mono.UsedCells);
            _grid.UnmarkOccupied(Mono);

            // 3) figure out shift
            int dRow = 0, dCol = 0;
            if      (direction == Vector3.forward)    dRow = +1;
            else if (direction == Vector3.back)       dRow = -1;
            else if (direction == Vector3.right)      dCol = +1;
            else if (direction == Vector3.left)       dCol = -1;

            // 4) build new cell list
            var targetCells = new List<(int row, int col)>();
            foreach (var (r, c) in oldCells)
                targetCells.Add((r + dRow, c + dCol));

            // 5) if any target cell is invalid or occupied, revert and bail
            bool outOfBounds = targetCells.Exists(tc =>
                tc.row < 0 || tc.row >= _grid.Rows ||
                tc.col < 0 || tc.col >= _grid.Cols
            );
            if (outOfBounds || _grid.IsCellsOccupied(targetCells))
            {
                Mono.IsBlocked = true;
                _grid.MarkOccupied(Mono, oldCells);
                Mono.ObjectAudio.StopSound();
                return;
            }
            
            var renderers = _grid.GetHighlightZones(oldCells);
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = false;
                var material = renderer.material;
                if (material == null) continue;
            }
            // 6) occupy new footprint and get world‐center
            Vector3 worldCenter = _grid.MarkOccupied(Mono, targetCells);

            // 7) tween from current to new center
            Mono.ObjectAudio.PlaySound();
            Mono.IsMoving = true;
            _grid.HighlightZones(oldCells, Mono.ImpactColor);
            _grid.UnhighlightZones(oldCells);
            _moveTween = Mono.transform
                .DOMove(worldCenter, Stats.timePerMove)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    // commit use of new cells
                    Mono.UsedCells = targetCells;
                    // re-mark so future checks see us
                    _grid.MarkOccupied(Mono, Mono.UsedCells);
                    
                    
                    _moveTween = null;
                    Mono.IsMoving = false;
                    _grid.HighlightZones(Mono.UsedCells, Mono.ImpactColor);
                });
        }
    }
}
