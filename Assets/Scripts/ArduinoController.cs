using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;

public class ArduinoController : MonoBehaviour {

    public float updateInterval = 200;

    private SerialPort stream;
    private PlayerControl pc;
    private HeatControl heatControl;

    private float windPercent = 0;
    private float heatPercent = 0;
    private float defaultHeatPercent = 0;

    private float maxWindHeatDelta = 0.3f;
    private float minWindForHeat = 0.1f;

    private float minPlayerSpeed;
    private float maxPlayerSpeed;

    private float lastUpdateTime;

    void Start()
    {
        try
        {
            stream = new SerialPort("COM3", 9600);
            stream.ReadTimeout = 50;
            stream.Open();
            Debug.Log("Connected to Arduino..");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        pc = GameComponents.GetPlayerControl();
        heatControl = GameComponents.GetGameController().GetComponent<HeatControl>();

        minPlayerSpeed = pc.GetMinSpeed();
        maxPlayerSpeed = pc.GetMaxSpeed();
        lastUpdateTime = Time.time * 1000;
    }

    void OnDestroy()
    {
        Send("w0h0");
        if (stream != null) stream.Close();
    }

    void Update()
    {
        if (!stream.IsOpen || Time.time * 1000 - lastUpdateTime < updateInterval) return;

        CalcWindStrength();
        CalcHeatStrength();

        Send("w" + windPercent + "h" + heatPercent);

        lastUpdateTime = Time.time * 1000;
    }


    private void Send(string message)
    {
        stream.Write(message);
        stream.BaseStream.Flush();
    }


    private void CalcWindStrength()
    {
        windPercent = (pc.GetCurrentSpeed() - minPlayerSpeed) / (maxPlayerSpeed - minPlayerSpeed) + 0.2f;
        if (pc.GetCurrentSpeed() < minPlayerSpeed)
        {
            windPercent = (pc.GetCurrentSpeed() / minPlayerSpeed) * 0.2f;
        }
        windPercent = Mathf.Clamp01(windPercent);
    }

    private void CalcHeatStrength()
    {
        heatPercent = heatControl.GetCurrentHeatPercent();

        // Safety mechanism
        if (windPercent < minWindForHeat)
        {
            heatPercent = 0;
        }
        else if (heatPercent > windPercent + maxWindHeatDelta)
        {
            heatPercent = windPercent + maxWindHeatDelta;
        }

        heatPercent = Mathf.Clamp01(heatPercent);
    }
}