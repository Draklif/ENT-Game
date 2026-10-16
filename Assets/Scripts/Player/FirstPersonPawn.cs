using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FirstPersonPawn : Pawn
{
    [Header("Movimiento")]
    public float walkSpeed = 4f;
    public float jumpHeight = 1f;
    public float gravity = 9.81f;

    [Header("Interacción")]
    [Tooltip("Distancia máxima (m) a la que se puede interactuar.")]
    public float interactRange = 3f;

    private CharacterController body;
    private float verticalSpeed;
    private float pitch;
    private Vector2 moveInput;
    private IInteractable target;

    void Awake()
    {
        body = GetComponent<CharacterController>();
    }

    public override void Move(Vector2 input) => moveInput = input;

    public override void Look(Vector2 input)
    {
        // Girar el cuerpo a los lados (yaw) y la cámara arriba/abajo (pitch).
        transform.Rotate(0f, input.x, 0f);
        pitch = Mathf.Clamp(pitch - input.y, -85f, 85f);
        pawnCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public override void Jump()
    {
        // v = √(2·g·h): velocidad inicial para alcanzar la altura jumpHeight.
        if (body.isGrounded) verticalSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);
    }

    public override void Interact()
    {
        target?.Interact(gameObject);
    }

    void Update()
    {
        if (body.isGrounded && verticalSpeed < 0f) verticalSpeed = -1f; // pegado al piso
        verticalSpeed -= gravity * Time.deltaTime;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = move * walkSpeed + Vector3.up * verticalSpeed;
        body.Move(velocity * Time.deltaTime);

        target = FindInteractable();
    }

    /// <summary>Lanza un rayo desde el centro de la cámara y busca un IInteractable.</summary>
    IInteractable FindInteractable()
    {
        Ray ray = new Ray(pawnCamera.transform.position, pawnCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            return hit.collider.GetComponentInParent<IInteractable>();
        return null;
    }
}
