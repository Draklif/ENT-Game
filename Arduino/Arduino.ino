// Pines de los potenciómetros
const int PIN_VELOCIDAD = A0;
const int PIN_TIMON = A1;

void setup() {
  Serial.begin(9600);
}

void loop() {
  int valorVelocidad = analogRead(PIN_VELOCIDAD); // 0-1023
  int valorTimon = analogRead(PIN_TIMON);         // 0-1023
  
  // Enviar como "velocidad,timon\n"
  Serial.print(valorVelocidad);
  Serial.print(",");
  Serial.println(valorTimon);
  
  delay(20); // ~50 Hz, suficiente para movimiento fluido
}