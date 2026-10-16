using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public FirstPersonPawn pawn;

    [Header("Simulación")]
    public TMP_Text timeText;
    public TMP_Text stepText;
    public TMP_Text stateText;
    public UnityEngine.UI.Image stateDot;

    [Header("Interacción")]
    public GameObject interactPrompt;
    public TMP_Text interactText;

    [Header("Colores")]
    public Color runningColor = new Color(0.28f, 0.73f, 0.47f);
    public Color pausedColor = new Color(0.96f, 0.68f, 0.33f);

    void Update()
    {
        Simulate sim = Simulate.Instance;
        timeText.text = $"{sim.time:F2} s";
        stepText.text = $"{sim.h:0.###} s";

        Color color = sim.paused ? pausedColor : runningColor;
        stateText.text = sim.paused ? "EN PAUSA" : "CORRIENDO";
        stateText.color = color;
        stateDot.color = color;

        IInteractable target = pawn.Target;
        interactPrompt.SetActive(target != null);
        if (target != null) interactText.text = target.GetInteractText();
    }
}
