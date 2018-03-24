#include <FS.h>                   //this needs to be first, or it all crashes and burns...
#include <ESP8266WiFi.h>          //https://github.com/esp8266/Arduino
#include <AsyncDelay.h>           //https://github.com/stevemarple/AsyncDelay

//needed for library
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include <WiFiManager.h>          //https://github.com/tzapu/WiFiManager

#include <ArduinoJson.h>          //https://github.com/bblanchon/ArduinoJson


std::unique_ptr<ESP8266WebServer> server;

//default custom static IP
char static_ip[16] = "192.168.1.99";
char static_gw[16] = "192.168.1.1";
char static_sn[16] = "255.255.255.0";

//flag for saving data
bool shouldSaveConfig = false;

// PINs
#define HEAT_PIN D1
#define WIND_PIN D2

// Constants
const int HEAT_CYCLE_TIME = 400; // in ms
const int WIND_CYCLE_TIME = 20; // in ms

// Variables
int heatCycleStartTime;
int windCycleStartTime;

float heatPercent = 0;
float windPercent = 0;
float vibrationPercent = 0;
int scentNum = 0; // 0 = no scent

struct feedbackData 
{
    float currentVal = 0;
    float defaultVal = 0;
    AsyncDelay revertChangeDelay;
};
typedef struct feedbackData FeedbackData;

struct requestData
{
  String type;
  float value;
  float duration;
};
typedef struct requestData RequestData;

FeedbackData heatData;

float defaultHeatPercent = 0;
AsyncDelay heatChangeDelay;

void loop() {
  server->handleClient();
  
  if (heatData.revertChangeDelay.isExpired()) heatPercent = defaultHeatPercent;

  pwm(WIND_PIN, WIND_CYCLE_TIME, windPercent, &windCycleStartTime, false);
  pwm(HEAT_PIN, HEAT_CYCLE_TIME, heatPercent, &heatCycleStartTime, false);
}

void initRoutes() {
  server->on("/", handleRoot);

  server->on("/led", []() {
    server->send(200, "text/plain", "test");
    tick();
  });

  server->on("/reset", []() {
    server->send(200, "text/plain", "resetting server..");
    resetWebServer();
  });

  server->on("/update", []() {

    for (int i = 0; i < server->args(); i++) {
      RequestData reqData = ExtractDataFromArgs(i);

      if (reqData.type == "w") {
        windPercent = constrain(reqData.value, 0, 1);
        Serial.println(reqData.type + " " + windPercent);
      }
      else if (reqData.type == "h") {
        UpdateFeedbackData(heatData, reqData);
      }
      else if (reqData.type == "s") {  
        scentNum = constrain((int)reqData.value, 0, 4);
        Serial.println(reqData.type + " " + scentNum);
      }
    }
    
    server->send(200, "text/plain", "temp update");
  });

  server->onNotFound(handleNotFound);
}

void handleRoot() {
  server->send(200, "text/plain", "hello from esp8266!");
}

void UpdateFeedbackData(FeedbackData fbData, RequestData reqData) {
  fbData.currentVal = constrain(reqData.value, 0, 1);

  if (reqData.duration == 0)
    fbData.defaultVal = fbData.currentVal;
  else 
    fbData.revertChangeDelay.start(reqData.duration * 1000, AsyncDelay::MILLIS);

  Serial.println(reqData.type + " " + fbData.currentVal);
}

RequestData ExtractDataFromArgs(int i) {
    RequestData data;

    data.type = server->argName(i);
    String argString = server->arg(i);
    int separatorIndex = argString.indexOf(",");

    data.value = argString.substring(0, separatorIndex).toFloat();
    data.duration = argString.substring(separatorIndex+1).toFloat();

    return data;
}

void tick()
{
  //toggle state
  int state = digitalRead(BUILTIN_LED);  // get the current state of GPIO1 pin
  digitalWrite(BUILTIN_LED, !state);     // set pin to the opposite state
}

void pwm(byte controlPin, int fullCycleTime, float onPercent, int *cycleStartTime, bool inverseCycle) {
  if(millis() - *cycleStartTime > fullCycleTime) {
    *cycleStartTime = millis();
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

void setup() {
  Serial.begin(115200);
  Serial.println();

  pinMode(HEAT_PIN, OUTPUT);
  pinMode(WIND_PIN, OUTPUT);
  digitalWrite(WIND_PIN, LOW);

  pinMode(BUILTIN_LED, OUTPUT);
  digitalWrite(BUILTIN_LED, HIGH);
 
  mountFSAndReadConfig();
  connectTheBadBoy();

  //if you get here you have connected to the WiFi
  Serial.println("connected...yeey :)");

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
  if (!wifiManager.autoConnect("BroomFeedbackSystemAP", "broomsday")) {
    Serial.println("failed to connect and hit timeout");
    delay(3000);
    //reset and try again, or maybe put it to deep sleep
    ESP.reset();
    delay(5000);
  }
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
        //static_ip = json["ip"];
        strcpy(static_ip, json["ip"]);
        strcpy(static_gw, json["gateway"]);
        strcpy(static_sn, json["subnet"]);
        //strcat(static_ip, json["ip"]);
        //static_gw = json["gateway"];
        //static_sn = json["subnet"];
        Serial.println(static_ip);
        /*Serial.println("converting ip");
        IPAddress ip = ipFromCharArray(static_ip);
        Serial.println(ip);*/
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

