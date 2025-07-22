using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.Decorator_Interface
{
    public abstract class ImpactObjectDecorator : IImpactObject
    {
        private readonly IImpactObject inner;
        protected MonoImpactObject Mono;
        protected ImpactObjectStats Stats;

        protected ImpactObjectDecorator(IImpactObject inner, MonoImpactObject mono, ImpactObjectStats stats)
        {
            this.inner = inner;
            Mono = mono;
            Stats = stats;
        }

        public virtual void StartImpact()
        {
           inner?.StartImpact();
        }

        public virtual void UpdateImpact(UnityEngine.Vector3 direction)
        {
            inner?.UpdateImpact(direction);
        }

        public virtual void DrawGizmos()
        {
            inner.DrawGizmos();
        }

        public virtual void StopImpact()
        {
           inner?.StopImpact();
        }
    }
}