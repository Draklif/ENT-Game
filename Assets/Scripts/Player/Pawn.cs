using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    [Tooltip("La cámara de este Pawn. Se activa cuando un PlayerController lo posee.")]
    public Camera pawnCamera;

    /// <summary>El controlador que lo posee ahora (null si nadie).</summary>
    public PlayerController Controller { get; private set; }

    public virtual void Move(Vector2 input) { }
    public virtual void Look(Vector2 input) { }
    public virtual void Jump() { }
    public virtual void Interact() { }

    public virtual void OnPossessed(PlayerController controller)
    {
        Controller = controller;
        if (pawnCamera != null) pawnCamera.gameObject.SetActive(true);
    }

    public virtual void OnUnpossessed()
    {
        Controller = null;
        if (pawnCamera != null) pawnCamera.gameObject.SetActive(false);
    }
}
