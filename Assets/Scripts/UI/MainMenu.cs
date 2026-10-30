using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú principal. Los botones llaman a estos métodos desde su On Click ().
/// Con control o Arduino siempre hay un botón seleccionado para poder navegar.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Tooltip("Escena que se carga con el botón Jugar. Debe estar en File > Build Profiles.")]
    public string gameScene = "Base";

    public GameObject mainPanel;
    public GameObject optionsPanel;

    private InputSystem_Actions input;

    void Awake()
    {
        input = new InputSystem_Actions();
        input.UI.Menu.performed += _ => Back(); // Esc / Start
        input.UI.Back.performed += _ => Back(); // B
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
    void OnDestroy() => input.Dispose();

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        ShowOptions(false);
    }

    void Update()
    {
        // Si se perdió la selección (ej: clic en el vacío), volver a seleccionar algo.
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy) SelectFirst(optionsPanel.activeSelf ? optionsPanel : mainPanel);
    }

    public void Play()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void ShowOptions(bool show)
    {
        mainPanel.SetActive(!show);
        optionsPanel.SetActive(show);
        SelectFirst(show ? optionsPanel : mainPanel);
    }

    /// <summary>Desde Opciones vuelve al menú. Lo usan Esc, Start, B y el botón "atrás" del Arduino.</summary>
    public void Back()
    {
        if (optionsPanel.activeSelf) ShowOptions(false);
    }

    public void Quit()
    {
        Application.Quit(); // en el editor no hace nada; funciona en el juego compilado
    }

    /// <summary>Selecciona el primer botón o slider del panel.</summary>
    void SelectFirst(GameObject panel)
    {
        Selectable first = panel.GetComponentInChildren<Selectable>();
        EventSystem.current.SetSelectedGameObject(first != null ? first.gameObject : null);
    }
}
