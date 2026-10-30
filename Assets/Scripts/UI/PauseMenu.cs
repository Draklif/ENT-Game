using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa (Esc / Start). Congela TODO el juego con Time.timeScale = 0
/// (la simulación también, porque corre en FixedUpdate) y suelta el mouse.
/// Los botones llaman a los métodos públicos desde su On Click ().
/// Con control o Arduino siempre hay un botón seleccionado para poder navegar.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("Se apaga mientras el menú está abierto, para que el jugador no se mueva ni gire.")]
    public PlayerController player;

    [Tooltip("El panel con los botones (Continuar, Opciones...).")]
    public GameObject pausePanel;

    [Tooltip("El panel con los sliders de ajustes.")]
    public GameObject optionsPanel;

    [Tooltip("Nombre de la escena del menú principal.")]
    public string mainMenuScene = "MainMenu";

    public bool IsOpen { get; private set; }

    private InputSystem_Actions input;

    void Awake()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();

        // Aplicar los ajustes guardados desde el inicio, aunque el panel de opciones esté oculto.
        foreach (SettingSlider setting in GetComponentsInChildren<SettingSlider>(true)) setting.Init();

        input = new InputSystem_Actions();
        input.UI.Menu.performed += _ => OpenOrBack(); // Esc / Start
        input.UI.Back.performed += _ =>               // B
        {
            if (IsOpen) Back();
        };

        Close();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
    void OnDestroy() => input.Dispose();

    void Update()
    {
        // Si se perdió la selección (ej: clic en el vacío), volver a seleccionar algo.
        if (!IsOpen) return;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy) SelectFirst(optionsPanel.activeSelf ? optionsPanel : pausePanel);
    }

    /// <summary>Cerrado: abre. Abierto: vuelve atrás. Lo usan Esc, Start y el botón "atrás" del Arduino.</summary>
    public void OpenOrBack()
    {
        if (IsOpen) Back();
        else Open();
    }

    /// <summary>Desde Opciones vuelve a la pausa; desde la pausa, cierra.</summary>
    public void Back()
    {
        if (optionsPanel.activeSelf) ShowOptions(false);
        else Close();
    }

    public void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        SelectFirst(pausePanel);
        if (player != null) player.enabled = false; // al apagarse suelta el cursor
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        if (player != null) player.enabled = true;  // al encenderse bloquea el cursor
    }

    public void ShowOptions(bool show)
    {
        pausePanel.SetActive(!show);
        optionsPanel.SetActive(show);
        SelectFirst(show ? optionsPanel : pausePanel);
    }

    public void ResetSimulation()
    {
        Simulate.Instance.ResetSimulation();
        Close();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>Selecciona el primer botón o slider del panel.</summary>
    void SelectFirst(GameObject panel)
    {
        Selectable first = panel.GetComponentInChildren<Selectable>();
        EventSystem.current.SetSelectedGameObject(first != null ? first.gameObject : null);
    }
}
