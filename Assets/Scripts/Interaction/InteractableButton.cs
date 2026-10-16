using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour, IInteractable
{
    public string text = "Usar";
    public UnityEvent onInteract = new UnityEvent();

    public void Interact(GameObject interactor)
    {
        onInteract.Invoke();
    }

    public string GetInteractText()
    {
        return text;
    }
}
