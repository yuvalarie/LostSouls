using System.Collections.Generic;
using Core.Managers;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;

namespace Game_Flow.DotVisual.Scripts.States
{
    public class FPState : IObjeckLockingState
    {
        private Transform _origin;
        private GameObject _dot;
        private Renderer _dotRenderer;
        private float _minDistance = .75f;
        private MonoImpactObject _target;
        
        public Vector3 DebugInputDir   { get;  set; }
        public MonoImpactObject DebugNextTarget { get; set; }

        public void EnterState(Transform origin, GameObject dotInstance, List<MonoImpactObject> impactObjects,
            ObjectController objectController)
        {
            EventManager.LockStateChanged(this);
            _origin = origin;
            _dot = dotInstance;
            _dotRenderer = _dot.GetComponent<Renderer>();
            
            Debug.Log("Entered FP state");
        }

        public void ExitState() { }

        public void Update()
        {
            // if (ObjectController.Instance.IsLocked)
            // {
            //     LockedUpdate();
            //     return;
            // }
            // UnLockedUpdate();
        }

        private void LockedUpdate()
        {
            _dot.transform.parent = _target?.transform;
        }

        private void UnLockedUpdate()
        {
            Vector3 from = _origin.position + _origin.transform.rotation *  Vector3.forward * _minDistance;
            if (Physics.Raycast(from, _origin.transform.rotation * Vector3.forward, out var hit))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("ImpactObject"))
                {
                    _dotRenderer.material.color = Color.green;
                    _target = hit.transform.gameObject.GetComponent<MonoImpactObject>();
                }
                else
                {
                    _dotRenderer.material.color = Color.red;
                    _target = null;
                }
                _dot.transform.position = hit.point;
            }
        }

        public MonoImpactObject GetTarget(out MonoImpactObject target)
        {
            // return target = _target;
            return target = null;

        }

        public Vector3 CalculateMovement(Vector2 input)
        {
            // return _origin.transform.rotation * (new Vector3(input.x, 0, input.y));
            return Vector3.zero;
        }
    }
}