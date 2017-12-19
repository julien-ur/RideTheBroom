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
    private float maxWindHeatDelta = 0.2f;
    private float minWindForHeat = 0.2f;

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
            Debug.LogError(e);
        }

        pc = GameComponents.GetPlayerControl();
        heatControl = GameComponents.GetGameController().GetComponent<HeatControl>();

        maxPlayerSpeed = pc.getMaxSpeed();
        lastUpdateTime = Time.time * 1000;
    }

    void OnDestroy()
    {
        Send("heat0");
        Send("wind0");
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
        windPercent = pc.getCurrentSpeed() / maxPlayerSpeed;
    }

    private void CalcHeatStrength()
    {
        heatPercent = heatControl.GetCurrentHeatPercent();

        // Safety mechanism
        if (windPercent < minWindForHeat)
        {
            heatPercent = 0;
        }
        else
        {
            heatPercent = Mathf.Min(heatPercent, windPercent + maxWindHeatDelta);
        }
    }
}