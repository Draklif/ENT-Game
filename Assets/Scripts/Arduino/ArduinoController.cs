using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Convierte los potenciómetros del Arduino en los controles del vehículo, igual que
/// los gatillos y el stick de un control: acelerador (0..1), freno (0..1), timón (-1..1).
/// Si el Arduino está conectado manda él; si no, se sigue usando teclado / control.
/// A pie (FirstPersonPawn) los potenciómetros no se usan.
/// Con 2 botones maneja los menús: toque = siguiente, mantener = seleccionar, otro botón = atrás.
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
    [Tooltip("Si el control no tiene freno, el Arduino debe mandar 0 en esa posición.")]
    public Axis brake = new Axis { index = 2 };

    [Header("Botones (0 = suelto, 1 = presionado)")]
    [Tooltip("Toque corto: siguiente botón del menú (en el juego: cambiar vista). Mantener: seleccionar.")]
    public int cycleButtonIndex = 3;

    [Tooltip("Atrás en los menús. En el juego abre el menú de pausa.")]
    public int backButtonIndex = 4;

    [Tooltip("Segundos manteniendo el botón de ciclar para seleccionar.")]
    public float holdToSelect = 0.6f;

    private bool cycleWasDown;
    private bool backWasDown;
    private bool holdDone;
    private float cycleDownTime;

    // LateUpdate corre después del Update del PlayerController, así el Arduino reemplaza al teclado.
    void LateUpdate()
    {
        ArduinoSerial arduino = ArduinoSerial.Instance;
        if (arduino == null || !arduino.IsConnected) return;

        // El Arduino pasa de escena en escena: buscar el jugador de la escena actual.
        if (player == null) player = FindAnyObjectByType<PlayerController>();

        Read(arduino, throttle, false);
        Read(arduino, brake, false);
        Read(arduino, steering, true);
        ReadButtons(arduino);

        if (player == null || player.pawn == null || player.pawn is FirstPersonPawn || !player.enabled) return;
        player.pawn.Drive(throttle.value, brake.value, steering.value);
    }

    void ReadButtons(ArduinoSerial arduino)
    {
        bool cycle = arduino.GetRaw(cycleButtonIndex) > 0;
        bool back = arduino.GetRaw(backButtonIndex) > 0;
        if (cycle || back) InputHint.UsingArduino = true;

        // Tiempo real (unscaled): en el menú de pausa Time.timeScale es 0.
        if (cycle && !cycleWasDown)
        {
            cycleDownTime = Time.unscaledTime;
            holdDone = false;
        }
        if (cycle && !holdDone && Time.unscaledTime - cycleDownTime >= holdToSelect)
        {
            holdDone = true;
            Submit();
        }
        if (!cycle && cycleWasDown && !holdDone) Cycle(); // se soltó antes de tiempo: toque corto
        if (back && !backWasDown) Back();

        cycleWasDown = cycle;
        backWasDown = back;
    }

    /// <summary>El botón o slider seleccionado en un menú abierto (null si no hay menú).</summary>
    GameObject MenuSelection()
    {
        if (EventSystem.current == null) return null;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        return selected != null && selected.activeInHierarchy ? selected : null;
    }

    void Cycle()
    {
        GameObject selected = MenuSelection();
        if (selected == null)
        {
            if (player != null && player.pawn != null && player.enabled) player.pawn.ChangeView();
            return;
        }

        // Bajar al siguiente; al llegar al final, volver al primero de arriba.
        Selectable current = selected.GetComponent<Selectable>();
        Selectable next = current.FindSelectableOnDown();
        if (next == null)
        {
            next = current;
            for (int i = 0; i < 50 && next.FindSelectableOnUp() != null; i++) next = next.FindSelectableOnUp();
        }
        EventSystem.current.SetSelectedGameObject(next.gameObject);
    }

    void Submit()
    {
        GameObject selected = MenuSelection();
        if (selected == null) return;

        // Un slider no se "presiona": cada vez que se mantiene sube 10 % y al pasar el máximo vuelve al mínimo.
        Slider slider = selected.GetComponent<Slider>();
        if (slider != null)
        {
            float step = (slider.maxValue - slider.minValue) * 0.1f;
            slider.value = slider.value + step > slider.maxValue + 0.001f ? slider.minValue : slider.value + step;
            return;
        }
        ExecuteEvents.Execute(selected, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    void Back()
    {
        PauseMenu pause = FindAnyObjectByType<PauseMenu>();
        if (pause != null)
        {
            pause.OpenOrBack();
            return;
        }
        MainMenu main = FindAnyObjectByType<MainMenu>();
        if (main != null) main.Back();
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
