// Control de vehículo para ENT-Game (ArduinoSerial.cs + ArduinoController.cs).
// Manda una línea por lectura: "acelerador,timon,freno\n" con valores 0-1023.
// Si no tienen freno, borren su lectura: Unity lo deja en 0.

// Pines de los potenciómetros
const int PIN_ACELERADOR = A0;
const int PIN_TIMON = A1;
const int PIN_FRENO = A2;

void setup() {
  Serial.begin(9600);
}

void loop() {
  Serial.print(analogRead(PIN_ACELERADOR)); // index 0
  Serial.print(",");
  Serial.print(analogRead(PIN_TIMON));      // index 1
  Serial.print(",");
  Serial.println(analogRead(PIN_FRENO));    // index 2

  delay(20); // ~50 Hz, suficiente para movimiento fluido
}
