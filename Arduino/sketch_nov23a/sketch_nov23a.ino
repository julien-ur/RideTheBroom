/* -------------------------------------------
/* CPU-Lüfter Steuern und messen
/* www.frag-duino.de
/*  modiefied by Julien Wachter
/* -------------------------------------------
/* Befehle:
/* 0-10 --> Länge des HIGH-Signals in ms*10
/* Langsam nach schnell: 1,2,3,4,5,6,7,8,9
/* 0 fuer STOP
 ------------------------------------------- */
 
// PINs
#define OUTPUT_PIN D2
#define INPUT_PIN D3
#define RELAY_PIN D1

// Constants
#define UPDATE_ZYKLUS 1000 // Jede Sekunde 1 ms Ausgabe der Geschwindigkeit.
const int ANZAHL_INTERRUPTS = 1; // Anzahl der Interrupts pro Umdrehung (1 oder 2)
const int HEAT_ON_INTERVAL = 200; // in ms
const int PWM_CYCLE_TIME = 20; // in ms
const float PWM_ON_PERCENT = 0;

// Variables for pwm
int counter_rpm = 0;
int rpm = 0;
unsigned long letzte_ausgabe = 0;
char eingabe;
int dauer_low = 1;
int dauer_high = 9;
int base_time = 10; // Insgesamt 10 ms

// Variables for relay
String read_string = "0";
bool new_string = false;
int on_delay = -1;
int off_delay = 0;

void setup()
{
  // Initialisieren
  pinMode(OUTPUT_PIN, OUTPUT);
  pinMode(INPUT_PIN, INPUT);
  pinMode(RELAY_PIN, OUTPUT);
  digitalWrite(INPUT_PIN, HIGH);
  attachInterrupt(0, rpm_fan, FALLING);
  Serial.begin(9600);
}
 
void loop(){
  Serial.println("loop");
  if(dauer_low * 10 != 0){
    digitalWrite(OUTPUT_PIN, LOW);
    delayMicroseconds(dauer_low * 10);
  }
 
  if(dauer_high * 10 != 0){
    digitalWrite(OUTPUT_PIN, HIGH);
    delayMicroseconds(dauer_high * 10);
  }

  if(millis() % on_delay == 0) {
    digitalWrite(RELAY_PIN, LOW);  // turn on relay with voltage LOW
  }
  if(millis() % off_delay == 0) {
    digitalWrite(RELAY_PIN, HIGH);  // turn off relay with voltage LOW
  }
  
  updateSerialInput();
 
  if (millis() - letzte_ausgabe >= UPDATE_ZYKLUS){
    // Interrupt deaktivieren um das rechnen nicht zu unterbrechen.
    detachInterrupt(0);
 
    // RPM errechnen und ausgeben:
    rpm = counter_rpm * (60 / ANZAHL_INTERRUPTS);
    Serial.print("RPM: ");
    Serial.println(rpm);
 
    // Counter zuruecksetzen
    counter_rpm = 0;
 
    // Zeitpunkt setzen
    letzte_ausgabe = millis();
 
    // Interrupt wieder aktivieren
    attachInterrupt(0, rpm_fan, FALLING);
  }
}

void updateSerialInput() {
  Serial.println("update input");
  if (Serial.available()) { read_string = ""; new_string = true; }
  
  while (Serial.available()) {
    delay(3);  //delay to allow buffer to fill 
    if (Serial.available() > 0) {
      char c = Serial.read();  //gets one byte from serial buffer
      read_string += c; //makes the string read_string
    }
  }

  if(new_string) {
    new_string = false;

    if (read_string.indexOf("wind") != -1) {
      int readInt = read_string.substring(5).toInt();

      readInt = (readInt == 0) ? 10 : 10 - readInt;
   
      dauer_low = eingabe;
      dauer_high = base_time - eingabe;
      Serial.print("PWM: Dauer des HIGH: ");
      Serial.println(dauer_high);
      
    }
    else if (read_string.indexOf("heat") != -1) {
      int readInt = read_string.substring(5).toInt();
      off_delay = (readInt == 0) ? 0 : readInt;
      on_delay = (readInt == 0) ? -1 : HEAT_ON_INTERVAL;

      Serial.print("RELAY: off delay: ");
      Serial.println(off_delay);
    }
  }
}
 
// Interrupt zaehlt den RPM-Counter hoch
void rpm_fan(){
  counter_rpm++;
}
