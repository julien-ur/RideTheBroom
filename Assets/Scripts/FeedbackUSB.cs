using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using NUnit.Framework.Constraints;
using UnityEngine;

public class FeedbackUSBEventArgs : EventArgs
{
    public string RawFeedbackData;
}

[ExecuteInEditMode]
public class FeedbackUSB : MonoBehaviour
{
    public EventHandler<FeedbackUSBEventArgs> FeedbackRequestSuccessful;

    private SerialPort serialPort;
    public bool sendTest = false;
    public string sendTestString = "w,0.5,2;";
    private Action callback;

    void OnDestroy()
    {
        StopAllFeedback();
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
        // print("Feedback Update " + rawData);

        if (rawData.Contains(FeedbackConstants.VIBRATION_TAG))
        {
            serialPort = new SerialPort("COM3");
        }
        else
        {
            serialPort = new SerialPort(@"\\.\COM12");
        }

        serialPort.BaudRate = 115200;

        try
        {
            OpenSerial();
            serialPort.Write(rawData);
            Debug.Log("Data sent " + rawData);
            callback = c;
            if (callback != null) callback();
            OnFeedbackRequestSuccessful(rawData);
            CloseSerial();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private void OpenSerial()
    {
        if (!serialPort.IsOpen) serialPort.Open();
    }

    private void CloseSerial()
    {
        if (serialPort.IsOpen) serialPort.Close();
    }

    public void PermanentUpdate(string tag, float val)
    {
        UpdateFeedback(tag + "," + val.ToString("0.00") + ";");
    }

    public void StopAllFeedback()
    {
        foreach (string fTag in FeedbackConstants.ALL_TAGS)
        {
            PermanentUpdate(fTag, 0);
        }
    }

    protected virtual void OnFeedbackRequestSuccessful(string rawData)
    {
        if (FeedbackRequestSuccessful != null)
            FeedbackRequestSuccessful(this, new FeedbackUSBEventArgs() { RawFeedbackData = rawData});
    }
}
