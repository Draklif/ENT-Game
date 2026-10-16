using System.Collections.Generic;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    public static Simulate Instance { get; private set; }

    [Tooltip("Paso de tiempo h en segundos.")]
    public float h = 0.02f;

    [Tooltip("Si está activo, la simulación no avanza.")]
    public bool paused = false;

    [Tooltip("Tiempo simulado t en segundos desde el último reinicio.")]
    public float time = 0f;

    private List<ISimulable> objects = new List<ISimulable>();

    void Awake()
    {
        Instance = this;
        Time.fixedDeltaTime = h;
    }

    void OnValidate()
    {
        // Al cambiar h en el inspector durante Play, se aplica de inmediato.
        if (Application.isPlaying && h > 0f) Time.fixedDeltaTime = h;
    }

    void Start()
    {
        // Busca todos los scripts de la escena que implementan ISimulable.
        foreach (MonoBehaviour script in FindObjectsByType<MonoBehaviour>())
        {
            if (script is ISimulable simulable) objects.Add(simulable);
        }
        ResetSimulation();
    }

    void FixedUpdate()
    {
        if (!paused) Step();
    }

    /// <summary>Avanza la simulación exactamente un paso h.</summary>
    public void Step()
    {
        foreach (ISimulable obj in objects) obj.Simulate(h);
        time += h;
    }

    public void TogglePause()
    {
        paused = !paused;
    }

    /// <summary>Lleva t a 0 y todos los objetos a su estado inicial.</summary>
    public void ResetSimulation()
    {
        time = 0f;
        foreach (ISimulable obj in objects) obj.ResetState();
    }

    /// <summary>Para objetos creados con Instantiate después de iniciar.</summary>
    public void Add(ISimulable obj)
    {
        objects.Add(obj);
        obj.ResetState();
    }
}
