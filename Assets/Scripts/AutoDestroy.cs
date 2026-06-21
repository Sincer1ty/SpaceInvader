using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void Start()
    {
        ParticleSystem particle = GetComponent<ParticleSystem>();
        
        Destroy(gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
    }
}
