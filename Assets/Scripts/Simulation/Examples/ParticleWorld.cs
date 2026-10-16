using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El mundo de las partículas. En cada paso h:
///   1. Calcula las fuerzas de cada partícula.
///   2. Integra (mueve) cada partícula.
///   3. Resuelve los choques entre partículas y contra el escenario.
/// </summary>
public class ParticleWorld : MonoBehaviour, ISimulable
{
    [Tooltip("Aceleración de la gravedad (m/s²).")]
    public Vector3 g = new Vector3(0f, -9.81f, 0f);

    private List<Particle> particles = new List<Particle>();

    void Awake()
    {
        particles.AddRange(FindObjectsByType<Particle>());
    }

    /// <summary>Para partículas creadas con Instantiate después de iniciar.</summary>
    public void Add(Particle p)
    {
        particles.Add(p);
        p.ResetState();
    }

    public void ResetState()
    {
        foreach (Particle p in particles) p.ResetState();
    }

    public void Simulate(float h)
    {
        // 1. Fuerzas.
        foreach (Particle p in particles)
        {
            p.force = Vector3.zero;
            p.AddForce(p.mass * g);
        }

        // 2. Integrar.
        foreach (Particle p in particles)
        {
            if (p.released) p.Integrate(h);
        }

        // 3. Choques. Cada par (i, j) se revisa una sola vez.
        for (int i = 0; i < particles.Count; i++)
        {
            for (int j = i + 1; j < particles.Count; j++)
            {
                CollideParticles(particles[i], particles[j]);
            }
        }
        foreach (Particle p in particles)
        {
            if (p.released) CollideWithScene(p, h);
        }

        foreach (Particle p in particles) p.ApplyPosition();
    }

    /// <summary>Choque entre dos esferas usando impulso y restitución.</summary>
    void CollideParticles(Particle a, Particle b)
    {
        if (!a.released || !b.released) return;

        Vector3 d = b.x - a.x;
        float distance = d.magnitude;
        float minDistance = a.Radius + b.Radius;
        if (distance >= minDistance || distance == 0f) return;

        Vector3 n = d / distance; // normal del choque, de a hacia b

        // Separarlas para que no queden una dentro de la otra (la más liviana se mueve más).
        float overlap = minDistance - distance;
        float totalMass = a.mass + b.mass;
        a.x -= n * overlap * (b.mass / totalMass);
        b.x += n * overlap * (a.mass / totalMass);

        // Velocidad relativa en la dirección del choque. Si es positiva, ya se están alejando.
        float vRel = Vector3.Dot(b.v - a.v, n);
        if (vRel > 0f) return;

        // Impulso: J = -(1 + e)·vRel / (1/ma + 1/mb)
        float e = (a.restitution + b.restitution) / 2f;
        float J = -(1f + e) * vRel / (1f / a.mass + 1f / b.mass);
        a.v -= (J / a.mass) * n;
        b.v += (J / b.mass) * n;
    }

    /// <summary>Choque contra cualquier Collider de la escena (piso, paredes, rampas).</summary>
    void CollideWithScene(Particle p, float h)
    {
        foreach (Collider other in Physics.OverlapSphere(p.x, p.Radius))
        {
            if (other.isTrigger || other.GetComponent<Particle>() != null) continue;

            // Unity nos dice hacia dónde empujar (n) y cuánto se metió (depth).
            if (!Physics.ComputePenetration(
                    p.Collider, p.x, p.transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out Vector3 n, out float depth)) continue;

            p.x += n * depth;

            // Rebote: v' = v - (1 + e)(v·n)n
            float vn = Vector3.Dot(p.v, n);
            if (vn >= 0f) continue;

            // Si choca más lento que lo que la gravedad acelera en ~2 pasos, ya está "en reposo": no rebota.
            float e = -vn < 2f * g.magnitude * h ? 0f : p.restitution;
            p.v -= (1f + e) * vn * n;
        }
    }
}
