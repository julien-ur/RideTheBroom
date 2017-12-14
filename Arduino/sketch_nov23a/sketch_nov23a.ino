// Föhn-Control für Arduino
// by Julien Wachter, 2017

// PINs
#define RELAY_PIN D1
#define PWM_PIN D2

// Constants
const int RELAY_CYCLE_TIME = 400; // in ms
const int PWM_CYCLE_TIME = 20; // in ms

// Variables
String serialInputString = "0";
bool serialInputAvailable = false;

float relayOnPercent = 0;
float pwmOnPercent = 0;
int relayCycleStartTime;
int pwmCycleStartTime;


void setup()
{
  pinMode(RELAY_PIN, OUTPUT);
  pinMode(PWM_PIN, OUTPUT);
  digitalWrite(PWM_PIN, LOW);
  
  Serial.begin(9600);

  pwmCycleStartTime = millis();
  relayCycleStartTime = millis();
}
 
void loop(){
  updateSerialInput();
  
  pwm(PWM_PIN, PWM_CYCLE_TIME, pwmOnPercent, &pwmCycleStartTime, false);
  pwm(RELAY_PIN, RELAY_CYCLE_TIME, relayOnPercent, &relayCycleStartTime, true);
}

void pwm(byte controlPin, int fullCycleTime, float onPercent, int *cycleStartTime, bool inverseCycle) {
  if(millis() - *cycleStartTime > fullCycleTime) {
    *cycleStartTime = millis();
  }
  
  if(millis() - *cycleStartTime < fullCycleTime * onPercent) {
    byte level = inverseCycle ? LOW : HIGH;
    digitalWrite(controlPin, level);
    
  } else {
    byte level = inverseCycle ? HIGH : LOW;
    digitalWrite(controlPin, level);
  }
}

void updateSerialInput() {
  if (Serial.available()) { serialInputString = ""; serialInputAvailable = true; }

  while (Serial.available()) {
    delay(3);  //delay to allow buffer to fill 
    if (Serial.available() > 0) {
      char c = Serial.read();  //gets one byte from serial buffer
      serialInputString += c; //makes the string serialInputString
    }
  }

  if(serialInputAvailable) {
    serialInputAvailable = false;

    if (serialInputString.indexOf("wind") != -1) {
      pwmOnPercent = serialInputString.substring(5).toFloat();
      pwmCycleStartTime = millis();
      
      Serial.print("PWM: Dauer des HIGH: ");
      Serial.println(PWM_CYCLE_TIME * pwmOnPercent);
    }
    else if (serialInputString.indexOf("heat") != -1) {
      relayOnPercent = serialInputString.substring(5).toFloat();
      relayCycleStartTime = millis();
      
      Serial.print("Relay: Dauer des HIGH: ");
      Serial.println(RELAY_CYCLE_TIME * relayOnPercent);
    }
  }
}
