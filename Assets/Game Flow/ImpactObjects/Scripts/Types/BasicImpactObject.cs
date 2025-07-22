using Game_Flow.ImpactObjects.Scripts.Decorator_Interface;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.Types
{
    public class BasicImpactObject : IImpactObject
    {
        private MonoImpactObject _mono;

        public BasicImpactObject(MonoImpactObject mono,ImpactObjectStats stats)
        {
            _mono = mono;
            
        }

        public void StartImpact()
        {
            
        }

        public void UpdateImpact(Vector3 direction)
        {
            
        }

        public void DrawGizmos()
        {
           
        }

        public void StopImpact()
        {
            
        }
    }
}