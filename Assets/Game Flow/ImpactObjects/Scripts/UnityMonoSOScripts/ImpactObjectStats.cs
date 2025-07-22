using UnityEngine;

namespace Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts
{
    [CreateAssetMenu(fileName = "NewScriptableObjectScript", menuName = "Scriptable Objects/ImpactObjectStats")]
    public class ImpactObjectStats : ScriptableObject
    {
        public float bufferForRaycast;
        public LayerMask impactObjectLayerMask;
        public LayerMask objectBorderLayerMask;
        public float timePerMove;
        public float littleDelay;
    }
}
