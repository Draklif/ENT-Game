using UnityEngine;

/// <summary>
/// Plantilla de vehículo. Se maneja con WASD o con el Arduino (ArduinoController).
/// E mirando el vehículo = subirse. E adentro = bajarse.
///
/// MODELO (lo que ya está hecho)
///    Estado:     x (posición), v (velocidad), heading (hacia dónde apunta, grados)
///    Entradas:   throttle (0..1), brake (0..1), steer (-1..1, timón)
///    Fuerzas:    F_motor = throttle · engineForce · adelante
///                F_freno = -brake · brakeForce · adelante  (quieto: reversa)
///    Ecuaciones: a = ΣF / m
///                v[n+1] = v[n] + a·h              (Euler semi-implícito, igual que Particle)
///                x[n+1] = x[n] + v[n+1]·h
///    Giro:       ω = v_adelante / L · tan(δ)       (modelo de bicicleta)
///
/// Ojo: sin fricción el vehículo nunca se detiene solo, y el agarre lateral es un
/// atajo que NO es física (ver ApplyGripPlaceholder). Simular eso de verdad es
/// justo lo que les toca en ComputeForces() y CollideWithScene().
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehiclePawn : Pawn, ISimulable, IInteractable
{
    [Header("Parámetros")]
    [Tooltip("Masa m (kg).")]
    public float mass = 1200f;

    [Tooltip("Fuerza máxima del motor (N).")]
    public float engineForce = 6000f;

    [Tooltip("Fuerza máxima del freno (N). Detenido, frenar da reversa con la mitad de engineForce.")]
    public float brakeForce = 9000f;

    [Tooltip("Agarre lateral SIMPLIFICADO (1/s). 0 = hielo. Reemplazarlo por fricción real con μ.")]
    public float gripPlaceholder = 6f;

    [Tooltip("Distancia entre ejes L (m).")]
    public float wheelBase = 2.5f;

    [Tooltip("Ángulo máximo de las llantas delanteras δ (grados).")]
    public float maxSteerAngle = 30f;

    [Header("Cámara y cabina")]
    [Tooltip("Puntos donde se pone la cámara; V cambia entre ellos. El primero es el inicial.")]
    public Transform[] views;

    [Tooltip("Opcional: este objeto gira con el timón (se ve desde la vista en 1ª persona).")]
    public Transform steeringWheel;

    [Header("Estado (solo lectura)")]
    public Vector3 x;
    public Vector3 v;
    public float heading;
    public float throttle;
    public float brake;
    public float steer;

    /// <summary>Hacia dónde apunta el vehículo.</summary>
    public Vector3 Forward => Quaternion.Euler(0f, heading, 0f) * Vector3.forward;

    /// <summary>Velocidad hacia adelante (m/s). Negativa = reversa.</summary>
    public float Speed => Vector3.Dot(v, Forward);

    /// <summary>Índice en views de la vista actual.</summary>
    public int CurrentView { get; private set; }

    private Rigidbody rb;
    private Vector3 x0;
    private float heading0;
    private Pawn driver; // quien se subió, para bajarlo después

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        x0 = transform.position;
        heading0 = transform.eulerAngles.y;
    }

    public override void Drive(float throttle, float brake, float steer)
    {
        this.throttle = throttle;
        this.brake = brake;
        this.steer = steer;
    }

    /// <summary>Pasa la cámara (con su HUD) al siguiente punto de vista.</summary>
    public override void ChangeView()
    {
        if (views.Length == 0) return;
        CurrentView = (CurrentView + 1) % views.Length;
        pawnCamera.transform.SetParent(views[CurrentView], false);
        pawnCamera.transform.localPosition = Vector3.zero;
        pawnCamera.transform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        // Volante visual: un timón real da ~1.5 vueltas hacia cada lado; aquí basta con 135°.
        if (steeringWheel != null) steeringWheel.localRotation = Quaternion.Euler(0f, 0f, -steer * 135f);
    }

    public void Simulate(float h)
    {
        // 1. Fuerzas.
        Vector3 F = ComputeForces();

        // 2. Integrar.
        Vector3 a = F / mass;
        v = v + a * h;
        x = x + v * h;

        // 3. Girar: ω = v / L · tan(δ). Quieto no gira, igual que un carro real.
        float delta = steer * maxSteerAngle * Mathf.Deg2Rad;
        float omega = Speed / wheelBase * Mathf.Tan(delta); // rad/s
        heading += omega * Mathf.Rad2Deg * h;
        ApplyGripPlaceholder(h);

        // 4. Choques.
        CollideWithScene();

        rb.MovePosition(x);
        rb.MoveRotation(Quaternion.Euler(0f, heading, 0f));
    }

    /// <summary>Suma de fuerzas ΣF. Aquí van los fenómenos del proyecto.</summary>
    Vector3 ComputeForces()
    {
        Vector3 F = Vector3.zero;

        // Motor: empuja hacia donde apunta el vehículo.
        F += throttle * engineForce * Forward;

        // Freno: se opone al avance. Casi detenido, el mismo pedal da reversa.
        if (Speed > 0.5f) F -= brake * brakeForce * Forward;
        else F -= brake * engineForce * 0.5f * Forward;

        // TODO Ideas de fenómenos (cada uno es una fuerza más):
        //   - Arrastre del aire:      F = -½·ρ·Cd·A·|v|·v
        //   - Resistencia rodadura:   F = -Crr·m·g·(v / |v|)
        //   - Agarre lateral (llantas): quitar la velocidad de lado (v · derecha). Si se pasa de
        //     un límite μ·m·g, la llanta se suelta y el carro derrapa.
        //   - Freno de mano: Jump() puede bajar el agarre de las llantas traseras.
        //   - Gravedad en rampas, empuje y sustentación (avión), etc.
        return F;
    }

    /// <summary>
    /// shortcut: NO es física. Gira la velocidad hacia donde apunta el carro para que no
    /// patine como en hielo, pero nunca derrapa. Reemplazar por fricción lateral real:
    /// F_lat = -(v · derecha)·k, limitada a μ·m·g; si se pasa del límite, derrapa.
    /// </summary>
    void ApplyGripPlaceholder(float h)
    {
        v = Vector3.Lerp(v, Forward * Speed, gripPlaceholder * h);
    }

    /// <summary>Choque contra el escenario (paredes, rampas, otros objetos).</summary>
    void CollideWithScene()
    {
        // TODO: ver ParticleWorld.CollideWithScene. Con Physics.OverlapBox y
        // Physics.ComputePenetration se saca la normal n; luego rebote: v' = v - (1 + e)(v·n)n
    }

    public void ResetState()
    {
        x = x0;
        v = Vector3.zero;
        heading = heading0;
        rb.position = x;
        transform.SetPositionAndRotation(x, Quaternion.Euler(0f, heading, 0f));
    }

    // E mirando el vehículo desde afuera: subirse.
    public void Interact(GameObject interactor)
    {
        Pawn pawn = interactor.GetComponent<Pawn>();
        if (pawn == null || pawn.Controller == null) return;

        driver = pawn;
        pawn.Controller.Possess(this);
        pawn.gameObject.SetActive(false);
    }

    public string GetInteractText()
    {
        return "Conducir";
    }

    // E estando adentro: bajarse por el lado izquierdo.
    public override void Interact()
    {
        if (driver == null) return;

        driver.transform.position = transform.position - transform.right * 2.5f + Vector3.up;
        driver.gameObject.SetActive(true);
        Controller.Possess(driver);
        driver = null;
        Drive(0f, 0f, 0f);
    }
}
