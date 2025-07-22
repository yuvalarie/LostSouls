using Core.Managers;
using UnityEngine;

namespace Game_Flow.Camera
{
    public class ViewZoneTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EventManager.TriggerPlayerZoneChanged(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EventManager.TriggerPlayerZoneChanged(false);
            }
        }
    }
}