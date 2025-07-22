
using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;

using UnityEngine;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class MovingShaderImpactObject: ImpactObjectDecorator
    {
        private Renderer[] _renderers;
        private readonly Color _impactColor;
        private readonly Color _lockedColor;
        private readonly Light _light;
        private readonly float _scale;
        private readonly float _width;
        private bool _once;
        private readonly Grid _grid;

        public MovingShaderImpactObject(IImpactObject inner, MonoImpactObject mono, ImpactObjectStats stats, Grid grid) : base(inner, mono, stats)
        {
            _grid = grid;
            _impactColor = mono.ImpactColor;
            _lockedColor = mono.LockedColor;
            _scale = mono.Scale;
            _width = mono.Width;
        }

        public override void StopImpact()
        {
            if (!Mono.IsMoving)
            {
                _grid.HighlightZones(Mono.UsedCells, _impactColor);
            }
        }

        public override void UpdateImpact(Vector3 direction)
        {
            base.UpdateImpact(direction);
            
        }
    }
}