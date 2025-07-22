using System.Collections.Generic;
using DG.Tweening;
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class ThreeBlockVerticalGridImpactObject : ImpactObjectDecorator
    {
        private readonly Grid   _grid;
        private Tween           _moveTween;

        public ThreeBlockVerticalGridImpactObject(
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

            // 1) if our own move‐tween is running, skip
            if (_moveTween != null && _moveTween.IsActive() && !_moveTween.IsComplete())
                return;

            // 2) unmark the old footprint so occupancy check ignores us
            var oldCells = new List<(int row, int col)>(Mono.UsedCells);
            _grid.UnmarkOccupied(Mono);

            // 3) compute shift
            int dRow = 0, dCol = 0;
            if      (direction == Vector3.forward) dRow = +1;
            else if (direction == Vector3.back)    dRow = -1;
            else if (direction == Vector3.right)   dCol = +1;
            else if (direction == Vector3.left)    dCol = -1;

            // 4) build target cells
            var targetCells = new List<(int row, int col)>();
            foreach (var (r, c) in oldCells)
                targetCells.Add((r + dRow, c + dCol));

            // 5) if any are OOB or occupied, revert & bail
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

            // 6) occupy new footprint & get its center
            Vector3 worldCenter = _grid.MarkOccupied(Mono, targetCells);
            Mono.ObjectAudio.PlaySound();
            Mono.IsMoving = true;
            _grid.HighlightZones(oldCells, Mono.ImpactColor);
            _grid.UnhighlightZones(oldCells);
            // 7) tween into position, then commit
            _moveTween = Mono.transform
                .DOMove(worldCenter, Stats.timePerMove)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Mono.UsedCells = targetCells;
                    _grid.MarkOccupied(Mono, Mono.UsedCells);
                    _moveTween = null;
                    Mono.IsMoving = false;
                    _grid.HighlightZones(Mono.UsedCells, Mono.ImpactColor);
                });
        }
    }
}
