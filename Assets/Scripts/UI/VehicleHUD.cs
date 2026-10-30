using TMPro;
using UnityEngine;

/// <summary>
/// HUD del vehículo. Es un Canvas en World Space pegado a la cámara del vehículo, así
/// cambia de vista junto con ella y también se ve en Cardboard / VR
/// (un Canvas "Screen Space - Overlay" no se dibuja en los lentes).
/// </summary>
public class VehicleHUD : MonoBehaviour
{
    public VehiclePawn vehicle;

    public TMP_Text speedText;
    public TMP_Text headingText;
    public TMP_Text inputText;
    public TMP_Text viewText;

    [Tooltip("Relleno de las barras. Se llenan desde el centro: -1 hacia un lado, +1 hacia el otro.")]
    public RectTransform throttleFill;
    public RectTransform steerFill;

    static readonly string[] directions = { "N", "NE", "E", "SE", "S", "SO", "O", "NO" };

    void LateUpdate()
    {
        speedText.text = $"{vehicle.Speed * 3.6f:0}"; // m/s → km/h (negativo = reversa)

        float angle = Mathf.Repeat(vehicle.heading, 360f);
        headingText.text = $"{directions[Mathf.RoundToInt(angle / 45f) % 8]}  {angle:000}°";

        SetBar(throttleFill, vehicle.throttle - vehicle.brake, true); // arriba acelera, abajo frena
        SetBar(steerFill, vehicle.steer, false);

        ArduinoSerial arduino = ArduinoSerial.Instance;
        if (arduino != null && arduino.IsConnected) inputText.text = $"ARDUINO  {arduino.ConnectedPort}";
        else inputText.text = InputHint.UsingGamepad ? "CONTROL" : "TECLADO";

        if (vehicle.views.Length > 0) viewText.text = vehicle.views[vehicle.CurrentView].name;
    }

    /// <summary>Estira el relleno entre el centro (0.5) y 0.5 + value/2.</summary>
    void SetBar(RectTransform fill, float value, bool vertical)
    {
        float from = Mathf.Min(0.5f, 0.5f + value / 2f);
        float to = Mathf.Max(0.5f, 0.5f + value / 2f);
        fill.anchorMin = vertical ? new Vector2(0f, from) : new Vector2(from, 0f);
        fill.anchorMax = vertical ? new Vector2(1f, to) : new Vector2(to, 1f);
    }
}
