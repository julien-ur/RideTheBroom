/*
 * Relay Shield - Blink
 * Turns on the relay for two seconds, then off for two seconds, repeatedly.
 *
 * Relay Shield transistor closes relay when D1 is HIGH
 */

const int relayPin = D1;
const long interval = 200;

String readString = "5000";
bool newString = false;
int onDelay;
int offDelay;

void setup() {
  pinMode(relayPin, OUTPUT);
  digitalWrite(relayPin, HIGH);
  Serial.begin(9600);
}

void loop() {

  if (Serial.available()) { readString = ""; newString = true; }
  
  while (Serial.available()) {
    delay(3);  //delay to allow buffer to fill 
    if (Serial.available() > 0) {
      char c = Serial.read();  //gets one byte from serial buffer
      readString += c; //makes the string readString
    }
  }

  if(newString) {
    newString = false;
    offDelay = readString.toInt();
    onDelay = (offDelay == 0) ? 1000 : interval;
    offDelay = (offDelay == 0) ? 0 : offDelay;
    Serial.println(offDelay);
  }

  digitalWrite(relayPin, HIGH); // turn off relay with voltage HIGH
  delay(offDelay);              // pause
  digitalWrite(relayPin, LOW);  // turn on relay with voltage LOW
  delay(onDelay);               // pause
}
