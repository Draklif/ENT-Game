using UnityEngine;

/// <summary>
/// Una partícula: una esfera con masa que se mueve por las fuerzas que recibe.
/// Es la base de casi todo: caída libre, tiro parabólico, resortes, tela, planetas.
///
/// MODELO
///    Estado:     x (posición), v (velocidad)
///    Parámetros: m (masa), r (radio), e (restitución), v0 (velocidad inicial)
///    Ecuaciones: a = ΣF / m
///                v[n+1] = v[n] + a·h          (Euler semi-implícito)
///                x[n+1] = x[n] + v[n+1]·h
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Particle : MonoBehaviour
{
    [Header("Parámetros")]
    public float mass = 1f;

    [Tooltip("Coeficiente de restitución e. 1 = rebote perfecto, 0 = no rebota.")]
    [Range(0f, 1f)] public float restitution = 0.8f;

    [Tooltip("Velocidad inicial v0 (m/s). Con componente horizontal = tiro parabólico.")]
    public Vector3 initialVelocity = Vector3.zero;

    [Tooltip("Si está activo, la partícula espera quieta hasta que alguien llame Release().")]
    public bool waitForRelease = false;

    [Header("Estado (solo lectura)")]
    public Vector3 x;
    public Vector3 v;
    public bool released;

    /// <summary>Suma de fuerzas de este paso (ΣF). ParticleWorld la llena.</summary>
    [HideInInspector] public Vector3 force;

    public SphereCollider Collider { get; private set; }
    public float Radius => Collider.radius * transform.lossyScale.x;

    private Rigidbody rb;
    private Vector3 x0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Collider = GetComponent<SphereCollider>();
        x0 = transform.position;
    }

    public void AddForce(Vector3 F)
    {
        force += F;
    }

    /// <summary>Lo puede llamar un botón (InteractableButton) para soltar la partícula.</summary>
    public void Release()
    {
        released = true;
    }

    public void Integrate(float h)
    {
        Vector3 a = force / mass;   // segunda ley de Newton
        v = v + a * h;
        x = x + v * h;
    }

    /// <summary>Mueve el objeto de Unity a la posición calculada.</summary>
    public void ApplyPosition()
    {
        rb.MovePosition(x);
    }

    public void ResetState()
    {
        x = x0;
        v = initialVelocity;
        force = Vector3.zero;
        released = !waitForRelease;
        rb.position = x;
        transform.position = x;
    }
}
