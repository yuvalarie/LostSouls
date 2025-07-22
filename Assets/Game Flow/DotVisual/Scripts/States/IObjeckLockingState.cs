using System.Collections.Generic;
using Core.Managers;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;

namespace Game_Flow.DotVisual.Scripts.States
{
    public interface IObjeckLockingState
    {
        void EnterState(Transform origin, GameObject dot, List<MonoImpactObject> impactObjects,
            ObjectController objectController);
        void ExitState();
        void Update();
        MonoImpactObject GetTarget(out MonoImpactObject target);
        Vector3 CalculateMovement(Vector2 input);
        
        
        public Vector3 DebugInputDir   { get; set; }
        public MonoImpactObject DebugNextTarget { get; set; }
    }
}