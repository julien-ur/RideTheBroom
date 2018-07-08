#include <FS.h>                   //this needs to be first, or it all crashes and burns...
#include <Utility.h>
#include <ESP8266WiFi.h>          //https://github.com/esp8266/Arduino
#include <AsyncDelay.h>           //https://github.com/stevemarple/AsyncDelay

//needed for library
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include <WiFiManager.h>          //https://github.com/tzapu/WiFiManager

#include <ArduinoJson.h>          //https://github.com/bblanchon/ArduinoJson

std::unique_ptr<ESP8266WebServer> server;

//default custom static IP
char static_ip[16] = "192.168.137.100";
char static_gw[16] = "192.168.137.1";
char static_sn[16] = "255.255.255.0";

//flag for saving data
bool shouldSaveConfig = false;

// PINs
#define HEAT_PIN D1
#define WIND_PIN D8
#define VIBRATION_PIN D4
#define SCENT_1_PIN D7
#define SCENT_2_PIN D6
#define SCENT_3_PIN D5
#define SCENT_4_PIN D0
#define RESET_PIN D3

// Constants
static const uint8_t OUTPUT_PINS[] = { HEAT_PIN, WIND_PIN, VIBRATION_PIN, SCENT_1_PIN, SCENT_2_PIN, SCENT_3_PIN, SCENT_4_PIN };
static const uint8_t *SCENT_PINS = OUTPUT_PINS + 3;
static const int HEAT_CYCLE_TIME = 400; // in ms
static const int WIND_CYCLE_TIME = 20; // in ms
static const int VIBRATION_CYCLE_TIME = 20; // in ms

// Variables
string serialInputString;
int heatCycleStartTime;
int windCycleStartTime;
int vibrationCycleStartTime;

struct requestData
{
  String type;
  float value;
  float duration;
};
typedef struct requestData RequestData;

struct feedbackData 
{
    String type;
    float currentVal = 0;
    float nextVal = 0;
    float defaultVal = 0;
    AsyncDelay *revertChangeDelay = NULL;
};
typedef struct feedbackData FeedbackData;

FeedbackData windData;
FeedbackData heatData;
FeedbackData vibrationData;
FeedbackData scentData;

void setup() {
  Serial.begin(115200);
  Serial.println();

  foreach(OUTPUT_PINS, sizeof(OUTPUT_PINS), pinMode, OUTPUT);
  foreach(OUTPUT_PINS, sizeof(OUTPUT_PINS), digitalWrite, LOW);
  
  pinMode(RESET_PIN, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(RESET_PIN), resetWebServer, HIGH);
  
  windData.type = "wind";
  heatData.type = "heat";
  vibrationData.type = "vibration";
  scentData.type = "scent";

  mountFSAndReadConfig();
  connectTheBadBoy();

  //save the custom parameters to FS
  if (shouldSaveConfig) saveConfig();
  
  server.reset(new ESP8266WebServer(WiFi.localIP(), 80));
  initRoutes();
  server->begin();
  
  Serial.println("HTTP server started");
  Serial.println(WiFi.localIP());
  Serial.println(WiFi.gatewayIP());
  Serial.println(WiFi.subnetMask());

  windCycleStartTime = millis();
  heatCycleStartTime = millis();
}

void handleRevertDelay(FeedbackData &d, void (*revertFun)(FeedbackData) = NULL) {
   if (d.revertChangeDelay && d.revertChangeDelay->isExpired()) {
    d.currentVal = d.defaultVal;
    d.nextVal = d.defaultVal;
    d.revertChangeDelay = NULL;
    if (revertFun) revertFun(d);
    Serial.println("reverted " + d.type + " to " + d.currentVal);
  }
}

void handleUpdate(string data) {
  RequestData data;
  data = ExtractDataFromSerialString(data);
  HandleRequest(data);
}

void loop() {
  server->handleClient();

  handleSerialInput();
  
  handleRevertDelay(windData);
  handleRevertDelay(heatData);
  handleRevertDelay(vibrationData);
  handleRevertDelay(scentData, &handleScent);
  
  pwm(WIND_PIN, WIND_CYCLE_TIME, &windData, &windCycleStartTime, false);
  pwm(HEAT_PIN, HEAT_CYCLE_TIME, &heatData, &heatCycleStartTime, false);
  pwm(VIBRATION_PIN, VIBRATION_CYCLE_TIME, &vibrationData, &vibrationCycleStartTime, false);
}

void initRoutes() {
  server->on("/", handleRoot);
  server->onNotFound(handleNotFound);

  server->on("/reset", []() {
    server->send(200, "text/plain", "resetting server..");
    resetWebServer();
  });

  server->on("/update", []() {
    
    for (int i = 0; i < server->args(); i++) {
      RequestData reqData = ExtractDataFromArgs(i);
      HandleRquest(reqData); 
    }
    server->send(200, "text/plain", "feedback updated");
  });
}

void handleSerialInput() {
  // data format: tag,strength,duration;
  if (Serial.available() > 0) {
    char c = Serial.read();  //gets one byte from serial buffer
    if (c == ';') {
      handleUpdate(serialInputString);
      serialInputString = '';
    }
    else {
      serialInputString += c; //makes the string serialInputString
    }
  }
}

void UpdateFeedbackSettings(FeedbackData *fbData, RequestData reqData) {
  if (fbData->type != "scent") {
    fbData->nextVal = constrain((float)reqData.value, 0, 1);
  } else {
    fbData->nextVal = constrain((int)reqData.value, 0, 4);
    fbData->currentVal = fbData->nextVal;
  }

  if (reqData.duration == 0 && fbData->type != "scent") {
    fbData->defaultVal = fbData->nextVal;
  } else {
    fbData->revertChangeDelay = new AsyncDelay((float)reqData.duration * 1000, AsyncDelay::MILLIS);
  }

  Serial.println(fbData->type + " val: " + fbData->nextVal + " dur: " + reqData.duration);
}

void HandleRequest(RequestData reqData) {
  if (reqData.type == "w") {
    UpdateFeedbackSettings(&windData, reqData);
  }
  else if (reqData.type == "h") {
    UpdateFeedbackSettings(&heatData, reqData);
  }
  else if (reqData.type == "v") {          
    UpdateFeedbackSettings(&vibrationData, reqData);
  }
  else if (reqData.type == "s") {          
    UpdateFeedbackSettings(&scentData, reqData);
    handleScent(scentData);
  }
}

RequestData ExtractDataFromSerialString(string s) {
  // data format: tag,strength,duration;
  RequestData data;
  char seperator = ',';
  int separatorIndex = s.indexOf(seperator);
  data.type = s.substring(0, seperatorIndex);

  int from = seperatorIndex;
  seperatorIndex = s.indexOf(seperator, from);
  data.value = s.substring(from, seperatorIndex);

  data.duration = s.substring(seperatorIndex+1);
}

RequestData ExtractDataFromArgs(int i) {
    RequestData data;

    data.type = server->argName(i);
    String argString = server->arg(i);
    int separatorIndex = argString.indexOf(",");

    data.value = argString.substring(0, separatorIndex);

    if (separatorIndex != -1) {
      data.duration = argString.substring(separatorIndex+1);
    } else {
      data.duration = 0;
    }

    return data;
}

void pwm(byte controlPin, int fullCycleTime, FeedbackData *data, int *cycleStartTime, bool inverseCycle) {
  float onPercent = data->currentVal;

  if(millis() - *cycleStartTime > fullCycleTime) {
    *cycleStartTime = millis();
    data->currentVal = data->nextVal;
  }
  
  byte level;
  if(millis() - *cycleStartTime < fullCycleTime * onPercent) {
    level = inverseCycle ? LOW : HIGH;
    digitalWrite(controlPin, level);
  } else {
    level = inverseCycle ? HIGH : LOW;
    digitalWrite(controlPin, level);
  }
}

void handleScent(FeedbackData scentData) {
  foreach(SCENT_PINS, sizeof(SCENT_PINS), digitalWrite, LOW);

  int scentNum = int(scentData.currentVal);
  if (scentNum != 0) {
    digitalWrite(SCENT_PINS[scentNum-1], HIGH);
  }
}

void connectTheBadBoy() {
  //WiFiManager
  //Local intialization. Once its business is done, there is no need to keep it around
  WiFiManager wifiManager;
  
  //set config save notify callback
  wifiManager.setSaveConfigCallback(saveConfigCallback);

  //set static ip
  IPAddress _ip,_gw,_sn;
  _ip.fromString(static_ip);
  _gw.fromString(static_gw);
  _sn.fromString(static_sn);

  wifiManager.setSTAStaticIPConfig(_ip, _gw, _sn);
  
  //reset settings - for testing
  //wifiManager.resetSettings();

  //set minimu quality of signal so it ignores AP's under that quality
  //defaults to 8%
  wifiManager.setMinimumSignalQuality();
  
  //sets timeout until configuration portal gets turned off
  //useful to make it all retry or go to sleep
  //in seconds
  //wifiManager.setTimeout(120);

  //fetches ssid and pass and tries to connect
  //if it does not connect it starts an access point with the specified name
  //here  "AutoConnectAP"
  //and goes into a blocking loop awaiting configuration
  if (!wifiManager.autoConnect("BroomsdayFeedbackSystemAP", "broomsday")) {
    Serial.println("failed to connect and hit timeout");
    delay(3000);
    //reset and try again, or maybe put it to deep sleep
    ESP.reset();
    delay(5000);
  }

  //if you get here you have connected to the WiFi
  Serial.println("connected...yeey :)");
}

void mountFSAndReadConfig() {
  //clean FS, for testing
  //SPIFFS.format();

  //read configuration from FS json
  Serial.println("mounting FS...");
  
  if (SPIFFS.begin()) {
    Serial.println("mounted file system");

    if (SPIFFS.exists("/config.json")) {
      //file exists, reading and loading
      readConfig();
    }
  } else {
    Serial.println("failed to mount FS");
  }
  //end read
}

void readConfig() {
  Serial.println("reading config file");
  File configFile = SPIFFS.open("/config.json", "r");
  
  if (configFile) {
    Serial.println("opened config file");
    size_t size = configFile.size();
    // Allocate a buffer to store contents of the file.
    std::unique_ptr<char[]> buf(new char[size]);

    configFile.readBytes(buf.get(), size);
    DynamicJsonBuffer jsonBuffer;
    JsonObject& json = jsonBuffer.parseObject(buf.get());
    json.printTo(Serial);
    if (json.success()) {
      Serial.println("\nparsed json");
      
      if(json["ip"]) {
        Serial.println("setting custom ip from config");
        strcpy(static_ip, json["ip"]);
        strcpy(static_gw, json["gateway"]);
        strcpy(static_sn, json["subnet"]);
        Serial.println(static_ip);
      } else {
        Serial.println("no custom ip in config");
      }
    } else {
      Serial.println("failed to load json config");
    }
  }
}

void saveConfig() {
  Serial.println("saving config");
  
  DynamicJsonBuffer jsonBuffer;
  JsonObject& json = jsonBuffer.createObject();

  json["ip"] = WiFi.localIP().toString();
  json["gateway"] = WiFi.gatewayIP().toString();
  json["subnet"] = WiFi.subnetMask().toString();

  File configFile = SPIFFS.open("/config.json", "w");
  if (!configFile) {
    Serial.println("failed to open config file for writing");
  }

  json.prettyPrintTo(Serial);
  json.printTo(configFile);
  configFile.close();
}

//callback notifying us of the need to save config
void saveConfigCallback () {
  Serial.println("Should save config");
  shouldSaveConfig = true;
}

void handleRoot() {
  server->send(200, "text/plain", "hello from broomsday feedback system!");
}

void handleNotFound() {
  String message = "File Not Found\n\n";
  message += "URI: ";
  message += server->uri();
  message += "\nMethod: ";
  message += (server->method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server->args();
  message += "\n";
  for (uint8_t i = 0; i < server->args(); i++) {
    message += " " + server->argName(i) + ": " + server->arg(i) + "\n";
  }
  server->send(404, "text/plain", message);
}

void resetWebServer() {
  if (digitalRead(BUILTIN_LED) == HIGH) {
    digitalWrite(BUILTIN_LED, LOW);
    Serial.println();
    delay(2000);
    Serial.println("closing connection..");
    server->close();
    Serial.println("resetting WifiManager..");
    WiFiManager wifiManager;
    wifiManager.resetSettings();
    delay(2000);
    setup();
  }
}

