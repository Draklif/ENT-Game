using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Una fila del menú de opciones: "Nombre ——o—— valor".
/// Cambia una variable pública (float) de CUALQUIER script, sin programar nada.
///
/// USO
///    1. Duplica una fila en el panel de opciones.
///    2. Arrastra el componente a "Target" (ej: el Particle de la esfera).
///    3. Escribe el nombre EXACTO de la variable en "Field" (ej: mass).
///    4. Ajusta Min Value / Max Value en el Slider.
/// </summary>
public class SettingSlider : MonoBehaviour
{
    [Tooltip("El script que tiene la variable. Ej: Particle, Simulate, PlayerController.")]
    public Component target;

    [Tooltip("Nombre EXACTO de la variable pública float. Ej: mass, h, lookSensitivity.")]
    public string field;

    [Tooltip("Opcional. Si tiene un nombre, el valor se guarda y se recuerda entre escenas " +
             "(ej: se cambia en el menú principal y se aplica al jugar).")]
    public string saveKey = "";

    public Slider slider;
    public TMP_Text valueText;

    private FieldInfo variable;
    private bool ready;

    void Awake()
    {
        Init();
    }

    /// <summary>Lee el valor inicial y aplica el guardado. El PauseMenu lo llama aunque el panel esté oculto.</summary>
    public void Init()
    {
        if (ready) return;
        ready = true;

        if (target != null)
        {
            // Reflection: buscar la variable por su nombre (texto) en el script.
            variable = target.GetType().GetField(field);
            if (variable == null) Debug.LogError($"{target.GetType().Name} no tiene una variable pública '{field}'.", this);
            else slider.value = (float)variable.GetValue(target);
        }

        if (saveKey != "" && PlayerPrefs.HasKey(saveKey)) slider.value = PlayerPrefs.GetFloat(saveKey);

        slider.onValueChanged.AddListener(OnChanged);
        OnChanged(slider.value);
    }

    void OnChanged(float value)
    {
        if (variable != null) variable.SetValue(target, value);
        if (saveKey != "") PlayerPrefs.SetFloat(saveKey, value);
        valueText.text = value.ToString("0.###");
    }
}
