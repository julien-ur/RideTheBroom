﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using NUnit.Framework.Constraints;
using UnityEngine;

class USBDeviceInfo
{
    public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
    {
        this.DeviceID = deviceID;
        this.PnpDeviceID = pnpDeviceID;
        this.Description = description;
    }
    public string DeviceID { get; private set; }
    public string PnpDeviceID { get; private set; }
    public string Description { get; private set; }
}

public class FeedbackUSB : MonoBehaviour
{
    private SerialPort serialPort;
    public bool sendTest = false;
    public string sendTestString = "w,0.5,2;";
    private Action callback;

    void OnDestroy()
    {
        UpdateFeedback("w,0;");
        UpdateFeedback("h,0;");
        UpdateFeedback("v,0;");
    }

    void Update()
    {
        if (sendTest)
            UpdateFeedback(sendTestString, () =>
            {
                Debug.Log("Data received");
            });
    }

    public void UpdateFeedback(string rawData, Action c=null)
    {
        sendTest = false;

        if (rawData.Contains(FeedbackConstants.VIBRATION_TAG))
        {
            serialPort = new SerialPort("COM3");
        }
        else
        {
            serialPort = new SerialPort(@"\\.\COM12");
        }

        serialPort.BaudRate = 115200;
        serialPort.DataReceived += DataRecievedHandler;

        OpenSerial();
        serialPort.Write(rawData);
        Debug.Log("Data sent");
        callback = c;
        if (callback != null) callback();
        CloseSerial();
    }

    protected virtual void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        string indata = sp.ReadExisting();
        Debug.Log(indata);
        if (callback != null)
        {
            callback();
            callback = null;
        }
        //CloseSerial();
    }

    public void OpenSerial()
    {
        if (!serialPort.IsOpen) serialPort.Open();
    }

    public void CloseSerial()
    {
        if (serialPort.IsOpen) serialPort.Close();
    }

    public void PermanentUpdate(string tag, float val)
    {
        UpdateFeedback(tag + "," + val + ";");
    }
}
