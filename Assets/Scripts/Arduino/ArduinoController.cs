using UnityEngine;

/// <summary>
/// Convierte los potenciómetros del Arduino en los controles del vehículo, igual que
/// los gatillos y el stick de un control: acelerador (0..1), freno (0..1), timón (-1..1).
/// Si el Arduino está conectado manda él; si no, se sigue usando teclado / control.
/// A pie (FirstPersonPawn) nunca se usa.
/// </summary>
public class ArduinoController : MonoBehaviour
{
    [System.Serializable]
    public class Axis
    {
        [Tooltip("Posición del número en la línea del Arduino (0 = el primero).")]
        public int index;

        [Tooltip("Valor crudo en reposo / extremo izquierdo. Calibrar mirando Raw en Play.")]
        public int min = 0;

        [Tooltip("Valor crudo a fondo / extremo derecho.")]
        public int max = 1023;

        public bool invert;

        [Tooltip("Zona muerta: cerca del reposo (o del centro, en el timón) cuenta como 0.")]
        [Range(0f, 0.3f)] public float deadZone = 0.05f;

        [Header("Solo lectura")]
        public int raw;
        public float value;
    }

    [Tooltip("Si se deja vacío, lo busca solo.")]
    public PlayerController player;

    public Axis throttle = new Axis { index = 0 };
    public Axis steering = new Axis { index = 1 };
    [Tooltip("Si el control no tiene freno, dejar un index que el Arduino no mande (queda en 0).")]
    public Axis brake = new Axis { index = 2 };

    void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();
    }

    // LateUpdate corre después del Update del PlayerController, así el Arduino reemplaza al teclado.
    void LateUpdate()
    {
        ArduinoSerial arduino = ArduinoSerial.Instance;
        if (arduino == null || !arduino.IsConnected) return;

        Read(arduino, throttle, false);
        Read(arduino, brake, false);
        Read(arduino, steering, true);

        if (player.pawn == null || player.pawn is FirstPersonPawn || !player.enabled) return;
        player.pawn.Drive(throttle.value, brake.value, steering.value);
    }

    /// <summary>Pedal: raw (min..max) → 0..1. Timón (centered): raw (min..max) → -1..1.</summary>
    void Read(ArduinoSerial arduino, Axis axis, bool centered)
    {
        axis.raw = arduino.GetRaw(axis.index);

        float t = Mathf.InverseLerp(axis.min, axis.max, axis.raw);
        if (axis.invert) t = 1f - t;
        if (centered) t = t * 2f - 1f;
        if (Mathf.Abs(t) < axis.deadZone) t = 0f;
        axis.value = t;
    }
}
