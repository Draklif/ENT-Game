using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Menú principal. Los botones llaman a estos métodos desde su On Click ().</summary>
public class MainMenu : MonoBehaviour
{
    [Tooltip("Escena que se carga con el botón Jugar. Debe estar en File > Build Profiles.")]
    public string gameScene = "Base";

    public GameObject mainPanel;
    public GameObject optionsPanel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        ShowOptions(false);
    }

    public void Play()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void ShowOptions(bool show)
    {
        mainPanel.SetActive(!show);
        optionsPanel.SetActive(show);
    }

    public void Quit()
    {
        Application.Quit(); // en el editor no hace nada; funciona en el juego compilado
    }
}
