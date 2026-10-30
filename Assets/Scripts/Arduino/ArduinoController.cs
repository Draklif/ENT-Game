using UnityEngine;

/// <summary>
/// Convierte los potenciómetros del Arduino en el input del vehículo, como si fueran WASD.
/// Si el Arduino está conectado manda él; si no, se sigue usando el teclado.
/// A pie (FirstPersonPawn) siempre se usa el teclado.
/// </summary>
public class ArduinoController : MonoBehaviour
{
    [System.Serializable]
    public class Axis
    {
        [Tooltip("Posición del número en la línea del Arduino (0 = el primero).")]
        public int index;

        [Tooltip("Valor crudo con el potenciómetro en un extremo (→ -1). Calibrar mirando Raw en Play.")]
        public int min = 0;

        [Tooltip("Valor crudo en el otro extremo (→ +1). Para una palanca que solo acelera, min = punto de reposo.")]
        public int max = 1023;

        public bool invert;

        [Tooltip("Zona muerta: cerca del centro cuenta como 0, para que no se mueva solo.")]
        [Range(0f, 0.3f)] public float deadZone = 0.05f;

        [Header("Solo lectura")]
        public int raw;
        public float value;
    }

    [Tooltip("Si se deja vacío, lo busca solo.")]
    public PlayerController player;

    public Axis throttle = new Axis { index = 0 };
    public Axis steering = new Axis { index = 1 };

    void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();
    }

    // LateUpdate corre después del Update del PlayerController, así el Arduino reemplaza al teclado.
    void LateUpdate()
    {
        ArduinoSerial arduino = ArduinoSerial.Instance;
        if (arduino == null || !arduino.IsConnected) return;

        Read(arduino, throttle);
        Read(arduino, steering);

        if (player.pawn == null || player.pawn is FirstPersonPawn || !player.enabled) return;
        player.pawn.Move(new Vector2(steering.value, throttle.value));
    }

    void Read(ArduinoSerial arduino, Axis axis)
    {
        axis.raw = arduino.GetRaw(axis.index);

        // raw (min..max) → -1..1
        float t = Mathf.InverseLerp(axis.min, axis.max, axis.raw) * 2f - 1f;
        if (axis.invert) t = -t;
        if (Mathf.Abs(t) < axis.deadZone) t = 0f;
        axis.value = t;
    }
}
