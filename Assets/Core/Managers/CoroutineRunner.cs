using UnityEngine;

namespace Core.Managers
{
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    }
}