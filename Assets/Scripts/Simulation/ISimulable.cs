public interface ISimulable
{
    /// <summary>Avanza el estado del objeto UN paso de tamaño h (segundos).</summary>
    void Simulate(float h);

    /// <summary>Vuelve al estado inicial (t = 0).</summary>
    void ResetState();
}
