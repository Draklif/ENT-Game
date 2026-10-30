// Control de vehículo para ENT-Game (ArduinoSerial.cs + ArduinoController.cs).
// Manda una línea por lectura: "acelerador,timon,freno,ciclo,atras\n"
//   - Potenciómetros: 0-1023.
//   - Botones: 1 = presionado, 0 = suelto.
// Si les falta algo (ej: no hay freno), manden 0 en esa posición para no correr las demás.
//
// Botones en el menú: CICLO toque corto = siguiente opción, mantener = seleccionar.
//                     ATRÁS = volver / abrir el menú de pausa.
// Botones en el juego: CICLO = cambiar vista.

// Pines de los potenciómetros
const int PIN_ACELERADOR = A0;
const int PIN_TIMON = A1;
const int PIN_FRENO = A2;

// Pines de los botones (cada botón va del pin a GND, sin resistencia)
const int PIN_CICLO = 2;
const int PIN_ATRAS = 3;

void setup() {
  Serial.begin(9600);
  pinMode(PIN_CICLO, INPUT_PULLUP); // con PULLUP el pin lee LOW cuando se presiona
  pinMode(PIN_ATRAS, INPUT_PULLUP);
}

void loop() {
  Serial.print(analogRead(PIN_ACELERADOR));           // index 0
  Serial.print(",");
  Serial.print(analogRead(PIN_TIMON));                // index 1
  Serial.print(",");
  Serial.print(analogRead(PIN_FRENO));                // index 2
  Serial.print(",");
  Serial.print(digitalRead(PIN_CICLO) == LOW ? 1 : 0); // index 3
  Serial.print(",");
  Serial.println(digitalRead(PIN_ATRAS) == LOW ? 1 : 0); // index 4

  delay(20); // ~50 Hz, suficiente para movimiento fluido
}
