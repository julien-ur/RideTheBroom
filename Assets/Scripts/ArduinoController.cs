using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;

public class ArduinoController : MonoBehaviour {

    private SerialPort stream;
    private PlayerControl pc;
    
    private int heatPercent = 0;
    private int windPercent = 0;
    private int defaultHeatPercent = 0;

    private float maxPlayerSpeed;

    void Start()
    {
        try
        {
            stream = new SerialPort("COM8", 9600);
            stream.ReadTimeout = 50;
            stream.Open();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        pc = GameComponents.GetPlayerControl();
        maxPlayerSpeed = pc.getMaxSpeed();
    }

    void OnDestroy()
    {
        if(stream != null) stream.Close();
    }

    void Update()
    {
        if (!stream.IsOpen) return;

        CalcWindStrength();

        Send("wind" + windPercent);
        Send("heat" + heatPercent);
    }


    private void CalcWindStrength()
    {
        windPercent = (int)(pc.getCurrentSpeed() * (100 / maxPlayerSpeed));
    }

    private void Send(string message)
    {
        stream.Write(message);
        stream.BaseStream.Flush();
    }


    public void SetHeatPercent(int h)
    {
        heatPercent = h;
    }

    public void SetDefaultHeatPercent(int h)
    {
        defaultHeatPercent = h;
    }

    public void ResetToDefaultHeat()
    {
        heatPercent = defaultHeatPercent;
    }
}