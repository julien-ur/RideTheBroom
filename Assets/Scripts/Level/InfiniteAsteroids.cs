using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteAsteroids : MonoBehaviour
{
    private Transform _tx;
    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;

    void Start()
    {
        _tx = transform;
        _ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int numParticlesAlive = _ps.GetParticles(_particles);
        for (int i = 0; i < numParticlesAlive; i++)
        {
            _particles[i].position += _tx.position;
        }
        _ps.SetParticles(_particles, numParticlesAlive);
    }
}
