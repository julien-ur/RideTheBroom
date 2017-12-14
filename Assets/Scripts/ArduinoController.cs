using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;

public class ArduinoController : MonoBehaviour {

    //public int strength = 0;
    //public bool send = false;

    private SerialPort stream;
    private PlayerControl pc;

    private int[] rawHeatValues = { 0, 25, 50, 100, 250, 500, 1000, 2000 };
    private int heat = 4;
    private int defaultHeat = 4;

    private float minPlayerSpeed, maxPlayerSpeed;

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
    }

    void OnDestroy()
    {
        if(stream != null) stream.Close();
    }

    void Update()
    {
        CalcWindStrength();
        CalcHeatStrength();

        if (!stream.IsOpen) return;

        Send("wind:" + CalcWindStrength().ToString());
        Send("heat:" + CalcHeatStrength().ToString());
        
        //if (send)
        //    Send(strength.ToString());
        //    send = false;
    }
        

    private int CalcWindStrength()
    {
        //Debug.Log(pc.getCurrentSpeed());
        return 1000;
    }

    private int CalcHeatStrength()
    {
        //Debug.Log(rawHeatValues[rawHeatValues.Length - 1 - heat]);
        return rawHeatValues[rawHeatValues.Length - 1 - heat];
    }

    private void Send(string message)
    {
        stream.Write(message);
        stream.BaseStream.Flush();
    }

    public void SetHeat(int h)
    {
        heat = h;
    }

    public void SetDefaultHeat(int h)
    {
        defaultHeat = h;
    }

    public void SetHeatToDefaultHeat()
    {
        heat = defaultHeat;
    }
}