using System.Collections.Generic;
using DG.Tweening;
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class TwoBlockVerticalGridImpactObject : ImpactObjectDecorator
    {
        private readonly Grid   _grid;
        private Tween           _moveTween;

        public TwoBlockVerticalGridImpactObject(
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

            // 1) if a move‐tween is still active, don’t start another
            if (_moveTween != null && _moveTween.IsActive() && !_moveTween.IsComplete())
                return;

            // 2) clear out the old two‐cell footprint
            var oldCells = new List<(int row, int col)>(Mono.UsedCells);
            _grid.UnmarkOccupied(Mono);

            // 3) compute target cells by shifting each old cell
            var targetCells = new List<(int row, int col)>();
            foreach (var (row, col) in oldCells)
            {
                int r = row, c = col;
                if      (direction == Vector3.forward) r += 1;
                else if (direction == Vector3.back)    r -= 1;
                else if (direction == Vector3.right)   c += 1;
                else if (direction == Vector3.left)    c -= 1;
                targetCells.Add((r, c));
            }

            // 4) check bounds/occupancy
            bool blocked = targetCells.Exists(tc =>
                tc.row < 0 || tc.row >= _grid.Rows ||
                tc.col < 0 || tc.col >= _grid.Cols
            ) || _grid.IsCellsOccupied(targetCells);

            if (blocked)
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

            // 5) occupy the new cells & get world center
            Vector3 newPos = _grid.MarkOccupied(Mono, targetCells);

            // 6) animate into place
            Mono.ObjectAudio.PlaySound();
            Mono.IsMoving = true;
            _grid.HighlightZones(oldCells, Mono.ImpactColor);
            _grid.UnhighlightZones(oldCells);
            _moveTween = Mono.transform
                .DOMove(newPos, Stats.timePerMove)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Mono.UsedCells = targetCells;
                    _grid.MarkOccupied(Mono, Mono.UsedCells);
                    _moveTween = null;
                    Mono.IsMoving = false;
                    _grid.HighlightZones(Mono.UsedCells, Mono.ImpactColor);
                    // commit new occupancy
                });
        }

        public override void StopImpact()
        {
            base.StopImpact();
            // no additional snapping required
        }
    }
}
