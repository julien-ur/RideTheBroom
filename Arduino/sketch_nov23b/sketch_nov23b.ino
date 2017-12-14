// PINs
#define PIN_BLAU 3
#define PIN_YELLOW 2

void setup() {
  pinMode(PIN_BLAU, OUTPUT);
  Serial.begin(9600);
}

void loop() {
  Serial.println("Test");
}
