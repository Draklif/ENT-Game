using UnityEngine;

/// <summary>
/// EJEMPLO: caída libre de una pelota.
///
/// 1. MODELO CONCEPTUAL
///    Una pelota se suelta desde una altura. La única fuerza es su peso,
///    así que acelera hacia abajo hasta tocar el piso. (Sin aire.)
///
/// 2. MODELO MATEMÁTICO
///    F = m·g   →   a = F/m = g          (g = (0, -9.81, 0) m/s²)
///    dv/dt = a          dx/dt = v
///    Solución exacta: y(t) = y0 + v0·t + ½·g·t²
///
/// 3. MODELO DE VARIABLES DE ESTADO
///    Estado:     x (posición), v (velocidad), t (tiempo desde que se soltó)
///    Parámetros: g, masa, v0 (velocidad inicial)
///    Paso h (Euler semi-implícito):
///        v[n+1] = v[n] + a·h
///        x[n+1] = x[n] + v[n+1]·h
/// </summary>

[RequireComponent(typeof(Rigidbody))]

public class CaidaLibre : MonoBehaviour, ISimulable
{
    [Header("Parámetros")]
    public Vector3 g = new Vector3(0f, -9.81f, 0f);
    public float mass = 1f;
    public Vector3 initialVelocity = Vector3.zero;

    [Tooltip("Altura (y) del piso")]
    public float floorY = 0f;

    [Header("Estado (solo lectura)")]
    public Vector3 x;
    public Vector3 v;
    public float t;
    public bool released;

    private Rigidbody rb;
    private Vector3 x0;
    private float radius;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        x0 = transform.position;
        radius = GetComponent<Collider>().bounds.extents.y;
    }

    /// <summary>Lo llama el botón (InteractableButton) para soltar la pelota.</summary>
    public void Release()
    {
        released = true;
    }

    public void ResetState()
    {
        x = x0;
        v = initialVelocity;
        t = 0f;
        released = false;
        rb.position = x;
        transform.position = x;
    }

    public void Simulate(float h)
    {
        if (!released) return;

        Vector3 F = mass * g;   // fuerza total
        Vector3 a = F / mass;   // segunda ley de Newton
        v = v + a * h;
        x = x + v * h;
        t = t + h;

        // Restricción física: la pelota no atraviesa el piso.
        if (x.y - radius <= floorY)
        {
            x.y = floorY + radius;
            released = false;

            // Validación contra la solución exacta t = √(2·d/g). Solo vale si v0 = 0.
            float drop = x0.y - x.y;
            float tExact = Mathf.Sqrt(2f * drop / -g.y);
            Debug.Log($"Impacto: t simulado = {t:F3} s, t exacto = {tExact:F3} s, error = {Mathf.Abs(t - tExact):F3} s (h = {h})");
        }

        rb.MovePosition(x);
    }
}
