using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageParticleBehaviour : MonoBehaviour
{
    private float duration = 0.0f;
    private float timer = 0.0f;
    private ParticleSystem particles = null;

    private void Start ()
    {
        particles = GetComponent<ParticleSystem>();
        duration = particles.main.duration;
	}
	
	private void Update ()
    {
        DestroyCounter();
	}

    private void DestroyCounter()
    {
        timer += Time.deltaTime;

        if(timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
