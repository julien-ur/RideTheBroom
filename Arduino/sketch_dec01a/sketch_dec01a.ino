/***  Declare constants and variables for the solar charger  ***/
#define controlPin D2       // Switch on/off for output voltage control
int percentVolt = 50;
int timeInMs = 20; //1ms: geht aus trotz 100%, 20ms: volle Leistung auf 100%, bei 15ms: leichter Abfall nach kurzer Zeit danach kontinuierlich langsamer

void setup() {
  pinMode(controlPin, OUTPUT);                     // Pin to control the output voltage
  digitalWrite(controlPin, LOW);           743,        // Switch on MOSFET to drive current to the output RC´
  Serial.begin(9600);
}

void loop() {
  String read_string = "";
  if(!Serial.available()) read_string = String(percentVolt);
  
  while (Serial.available()) {
    delay(3);  //delay to allow buffer to fill 
    if (Serial.available() > 0) {
      char c = Serial.read();  //gets one byte from serial buffer
      read_string += c; //makes the string read_string
    }
  }
  percentVolt = read_string.toInt();
  
  Serial.print(percentVolt);
  Serial.print(" ");
  Serial.print(timeInMs * (percentVolt/100.0));
  Serial.print(" ");
  Serial.println(timeInMs * (1-(percentVolt/100.0)));
  
  digitalWrite(controlPin, HIGH);
  delay(timeInMs * (percentVolt/100.0));
  digitalWrite(controlPin, LOW);
  delay(timeInMs * (1-(percentVolt/100.0)));
}
