using System;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;

namespace Game_Flow.DotVisual.Scripts
{
    //comment to merge conflict
    public class DotVisualController : MonoBehaviour
    {
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float rayLength = 50f;
        [SerializeField] private float rayOffset = 0.5f;
        
        [SerializeField] private LayerMask targetLayers;
        
        [SerializeField] private GameObject dotPrefab;
        
        [SerializeField] private Color defaultColor = Color.red;
        [SerializeField] private Color hitColor = Color.green;
        
        private Renderer dotRenderer;
        private GameObject dotInstance;
        private const float SphereRadius = 1f;
        
        private bool _isInTopDownMode;
        
        public MonoImpactObject CurrentTarget { get; private set; }
        public bool IsLocked { get; set; } = false;

        public void Start()
        {
            if (dotPrefab != null)
            {
                dotInstance = Instantiate(dotPrefab);
                dotRenderer = dotInstance.GetComponent<Renderer>();
                dotRenderer.material.color = defaultColor;
            }
        }
        
        public void Update()
        {
            if (rayOrigin.parent == null)
            {
                Debug.LogWarning("rayOrigin has lost its parent!");
            }
            if (IsLocked && CurrentTarget != null)
            {
                dotInstance.transform.position = CurrentTarget.transform.position + CurrentTarget.transform.localScale / 2;
                dotRenderer.material.color = hitColor;
                return;
            }
            
            RaycastHit hit;
            Vector3 direction = (rayOrigin.forward + Vector3.down * rayOffset).normalized;

            Debug.DrawRay(rayOrigin.position, direction * rayLength, Color.red);
            if (Physics.SphereCast(rayOrigin.position, SphereRadius, direction, out hit, rayLength, targetLayers))
            {
                dotInstance.transform.position = hit.point;
                var impactObject = hit.collider.GetComponent<MonoImpactObject>();
                if (impactObject != null)
                {
                    CurrentTarget = impactObject;
                    dotRenderer.material.color = hitColor;
                }
                else
                {
                    CurrentTarget = null;
                    dotRenderer.material.color = defaultColor;
                }
            }
            else
            {
                dotInstance.transform.position = rayOrigin.position + rayOrigin.forward * rayLength;
                CurrentTarget = null;
                dotRenderer.material.color = defaultColor;
            }
        }
        
        
        public void MoveRayOrigin(Vector3 direction)
        {
            Vector3 localMove = new Vector3(direction.x, 0, 0); // block Z
            rayOrigin.localPosition += localMove * Time.deltaTime * 5f;

            rayOrigin.localPosition = new Vector3(
                Mathf.Clamp(rayOrigin.localPosition.x, -10f, 10f),
                0,
                0
            );
        }
        
        public void ResetRayOriginLocalPosition()
        {
            if (rayOrigin.parent != null)
            {
                rayOrigin.position = rayOrigin.parent.position;
            }
            else
            {
                Debug.LogWarning("Ray origin has no parent!");
            }
        }
        
        private void OnDrawGizmos()
        {
            if (rayOrigin != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(rayOrigin.position, 0.2f);
            }
        }
        
        public void SetTopDownMode(bool isTopDown)
        {
            _isInTopDownMode = isTopDown;
        }
        
        public Vector3 GetRayOriginPosition() => rayOrigin.position;
        public void SetRayOriginPosition(Vector3 position) => rayOrigin.localPosition += position;
    }
}