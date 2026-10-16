using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Tooltip("El Pawn que este controlador posee al iniciar.")]
    public Pawn pawn;

    [Tooltip("Sensibilidad del mouse para mirar.")]
    public float lookSensitivity = 0.1f;

    // Clase generada automáticamente desde Assets/InputSystem_Actions.inputactions
    private InputSystem_Actions input;

    void Awake()
    {
        input = new InputSystem_Actions();

        // Botones: se ejecutan UNA vez cuando se presiona la tecla.
        input.Player.Jump.performed += _ => pawn?.Jump();
        input.Player.Interact.performed += _ => pawn?.Interact();

        input.Simulation.Pause.performed += _ => Simulate.Instance.TogglePause();
        input.Simulation.Step.performed += _ => Simulate.Instance.Step();
        input.Simulation.Reset.performed += _ => Simulate.Instance.ResetSimulation();
    }

    void Start()
    {
        if (pawn != null) Possess(pawn);
    }

    void OnEnable()
    {
        input.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        input.Disable();
        Cursor.lockState = CursorLockMode.None;
    }

    void OnDestroy()
    {
        input.Dispose();
    }

    void Update()
    {
        if (pawn == null) return;

        // Valores continuos: se leen en cada frame.
        pawn.Move(input.Player.Move.ReadValue<Vector2>());
        pawn.Look(input.Player.Look.ReadValue<Vector2>() * lookSensitivity);
    }

    /// <summary>
    /// Toma el control de otro Pawn. Ej: al subirse a un carro,
    /// el carro llama a controller.Possess(this).
    /// </summary>
    public void Possess(Pawn newPawn)
    {
        if (pawn != null) pawn.OnUnpossessed();
        pawn = newPawn;
        pawn.OnPossessed(this);
    }
}
