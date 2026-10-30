using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Muestra la tecla o el botón de una acción: "E" con teclado, "Y" con control.
/// Cambia solo según lo último que se presionó.
/// USO: ponerlo en un texto y arrastrar la acción (ej: Player/Interact) desde InputSystem_Actions.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class InputHint : MonoBehaviour
{
    /// <summary>True si lo último que se presionó fue un botón del control.</summary>
    public static bool UsingGamepad { get; private set; }

    /// <summary>True si lo último que se presionó fue un botón del Arduino (lo pone ArduinoController).</summary>
    public static bool UsingArduino;

    [Tooltip("La acción, arrastrada desde Assets/InputSystem_Actions (despliega la flecha del archivo).")]
    public InputActionReference action;

    [Tooltip("{0} se reemplaza por la tecla. Ej: \"{0}\" o \"[{0}] Vista\".")]
    public string format = "{0}";

    [Tooltip("Opcional: qué mostrar si se está usando el Arduino. Ej: \"BOTÓN 1\". Vacío = no cambia.")]
    public string arduinoKey = "";

    private TMP_Text text;

    [RuntimeInitializeOnLoadMethod]
    static void ListenForDevice()
    {
        InputSystem.onAnyButtonPress.Call(control =>
        {
            UsingGamepad = control.device is Gamepad;
            UsingArduino = false;
        });
    }

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        string group = UsingGamepad ? "Gamepad" : "Keyboard&Mouse";
        string key = UsingArduino && arduinoKey != "" ? arduinoKey : KeyName(action.action, group);
        text.text = string.Format(format, key);
    }

    /// <summary>Nombre corto del primer botón de la acción en ese grupo ("E", "Y", "Start", "RB").</summary>
    static string KeyName(InputAction action, string group)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isComposite && !binding.isPartOfComposite && binding.groups.Contains(group))
                return action.GetBindingDisplayString(i);
        }
        return "?";
    }
}
