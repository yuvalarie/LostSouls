using UnityEngine;

namespace Game_Flow.ParticleSystem
{
    public static class DollParticles
    {
        public static void OnDollPlaced(UnityEngine.ParticleSystem p1, UnityEngine.ParticleSystem p2, UnityEngine.ParticleSystem p3, UnityEngine.ParticleSystem p4)
        {
            Debug.Log("particles stuff");
            p1.Stop();
            p2.Stop();
            p3.gameObject.SetActive(true);
            p4.gameObject.SetActive(true);
            p3.Play();
            p4.Play();
        }
    }
}