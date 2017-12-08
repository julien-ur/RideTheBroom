using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;


public class ArduinoController : MonoBehaviour {

    public int strength = 0;
    public bool send = false;

    private SerialPort stream;
    private PlayerControl pc;

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
        if (!stream.IsOpen) return;

        Send("wind:" + CalcWindStrength().ToString());
        Send("heat:" + CalcHeatStrength().ToString());
        
        if (send)
            Send(strength.ToString());
            send = false;
    }
        

    private int CalcWindStrength()
    {
        Debug.Log(pc.getCurrentSpeed());
        return 1000;
    }

    private int CalcHeatStrength()
    {
        return 12;
    }

    private void Send(string message)
    {
        stream.Write(message);
        stream.BaseStream.Flush();
    }
}