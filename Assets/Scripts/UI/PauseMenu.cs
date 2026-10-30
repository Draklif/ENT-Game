using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú de pausa (tecla Esc). Congela TODO el juego con Time.timeScale = 0
/// (la simulación también, porque corre en FixedUpdate) y suelta el mouse.
/// Los botones llaman a los métodos públicos desde su On Click ().
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
        input.UI.Cancel.performed += _ => Toggle();

        Close();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
    void OnDestroy() => input.Dispose();

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        if (player != null) player.enabled = false; // al apagarse suelta el cursor
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        if (player != null) player.enabled = true;  // al encenderse bloquea el cursor
    }

    public void ShowOptions(bool show)
    {
        pausePanel.SetActive(!show);
        optionsPanel.SetActive(show);
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
}
