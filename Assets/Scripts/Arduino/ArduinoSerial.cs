using System.IO.Ports;
using System.Threading;
using UnityEngine;

/// <summary>
/// Lee del Arduino líneas de números separados por coma, ej: "512,300\n" (ver Arduino/Arduino.ino).
/// Todo corre en un hilo aparte para que el juego no se congele esperando al puerto.
/// Requiere Project Settings > Player > Api Compatibility Level = .NET Framework.
/// </summary>
public class ArduinoSerial : MonoBehaviour
{
    public static ArduinoSerial Instance { get; private set; }

    [Tooltip("Ej: COM3. Vacío = probar todos los puertos hasta encontrar el Arduino.")]
    public string portName = "";
    public int baudRate = 9600;

    public bool IsConnected => connected;
    public string ConnectedPort => connectedPort;

    private volatile bool running;
    private volatile bool connected;
    private volatile string connectedPort;
    private volatile int[] values = new int[0]; // se reemplaza completo, nunca a medias
    private Thread thread;
    private SerialPort serial;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        running = true;
        thread = new Thread(ReadLoop) { IsBackground = true };
        thread.Start();
    }

    void OnDisable()
    {
        running = false;
        try { serial?.Close(); } catch { } // desbloquea ReadLine
        thread.Join(500);
    }

    /// <summary>Valor crudo número i de la línea (0-1023 con analogRead). 0 si no ha llegado.</summary>
    public int GetRaw(int i)
    {
        int[] current = values;
        return i < current.Length ? current[i] : 0;
    }

    // ---------- Hilo de lectura ----------

    void ReadLoop()
    {
        while (running)
        {
            string[] ports = portName != "" ? new[] { portName } : SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                if (!running) return;
                ReadPort(port);
            }
            Thread.Sleep(1000); // nadie respondió: esperar y volver a buscar
        }
    }

    void ReadPort(string port)
    {
        int badLines = 0;
        try
        {
            // El Arduino se reinicia al abrir el puerto y tarda ~2 s en mandar datos.
            serial = new SerialPort(port, baudRate) { ReadTimeout = 2500, NewLine = "\n" };
            serial.Open();
            while (running && badLines < 5)
            {
                if (Parse(serial.ReadLine()))
                {
                    badLines = 0;
                    if (!connected) Debug.Log($"[Arduino] Conectado en {port}");
                    connectedPort = port;
                    connected = true;
                }
                else badLines++; // otro aparato que no habla nuestro formato
            }
        }
        catch (System.Exception) { } // puerto ocupado, sin datos o desconectado: probar el siguiente
        finally
        {
            try { serial?.Close(); } catch { }
            if (connected) Debug.Log($"[Arduino] Desconectado de {port}");
            connected = false;
            connectedPort = null;
        }
    }

    bool Parse(string line)
    {
        string[] parts = line.Trim().Split(',');
        int[] parsed = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out parsed[i])) return false;
        }
        values = parsed;
        return true;
    }
}
