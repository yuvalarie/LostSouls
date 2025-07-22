namespace Game_Flow.ImpactObjects.Scripts.Decorator_Interface
{
    public interface IImpactObject
    {
        public void StartImpact();
        public void UpdateImpact(UnityEngine.Vector3 direction);
        public void DrawGizmos();
        public void StopImpact();
    }
}