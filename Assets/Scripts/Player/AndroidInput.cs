using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using System.Diagnostics;

using System.IO.Ports;

public class AndroidInput : MonoBehaviour
{
	private const int PACKET_LENGTH = 6; // in bytes
	public enum InputMode {WiFi, USB};

	//public string ipAdress = "192.168.0.232";
	public int port = 5005;

    private bool serverError = false;
    private bool verticalInputError = false;
    private bool horizontalInputError = false;

    public float inputDivisorVertical = 45.0f; // 45
    public float inputDivisorHorizontal = 60.0f; // 60
    public float neutralAreaHorizontal = 10.0f; // 10

    public InputMode AndroidInputMode = InputMode.WiFi;

    private volatile float firstZ = 0;
    private volatile float firstY = 0;
    private volatile bool isZset = false;
    private volatile bool isYset = false;

    private float time = 0;
    private float lastTime = 0;

    private System.Diagnostics.Stopwatch stopwatch;

    private Thread udpThread;
    private UdpClient client;
    private volatile bool runUdpThread;

    private Thread usbThread;
    private SerialPort usbPort;
    private volatile bool runUsbThread;

    private volatile short x, y, z;

	void Start ()
	{
		//stopwatch = new System.Diagnostics.Stopwatch();
		//stopwatch.Start();
		if(AndroidInputMode == InputMode.WiFi)
		{
			udpThread = new Thread( new ThreadStart(ReceiveDataUDP) );
			udpThread.IsBackground = true;
			udpThread.Start();
			runUdpThread = true;
		}
		else
		{
			usbThread = new Thread( new ThreadStart(ReceiveDataUSB) );
			usbThread.IsBackground = true;
			usbThread.Start();
			runUdpThread = true;
		}
		
	}
	
	void Update ()
	{
		//Debug.Log("x: " + x + ", y: " + y + ", z: " + z);
	}

	void OnDestroy()
	{
		runUdpThread = false;
		runUsbThread = false;
    }

    private void ReceiveDataUSB()
    {
    	usbPort = new SerialPort();
		usbPort.PortName = "COM1";	// change this!
        usbPort.Parity = Parity.None;
        usbPort.BaudRate = 9600;
        usbPort.DataBits = 8 * PACKET_LENGTH;
        usbPort.StopBits = StopBits.One;

        int bufCount = 0;
        byte[] data = new byte[PACKET_LENGTH];

        usbPort.Open();

        while(runUsbThread)
        {
        	try
			{
				//IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
				//byte[] data = client.Receive(ref ip);

				bufCount += usbPort.Read(data, bufCount, data.Length - bufCount);

				if(bufCount < PACKET_LENGTH) continue;

				bufCount = 0;
				data = new byte[PACKET_LENGTH];

                x = (short) ( (data[1] << 8) | data[0] );
				y = (short) ( (data[3] << 8) | data[2] );
				z = (short) ( (data[5] << 8) | data[4] );

                //Debug.LogError("x: " + x + " y: " + y + " z: " + z);

				/*if(y == lastY)
				{
					Debug.Log("--- SAME ---");
				}
				else
				{
					lastY = y;
				}*/

				if(!isZset)
				{
					firstZ = z;
					isZset = true;
				}

				if(!isYset)
				{
					firstY = y;
					isYset = true;
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
				//serverError = true;
			}
        }
        usbPort.Close();
    }

	private void ReceiveDataUDP()
	{
		client = new UdpClient(port);

		//short lastY = 0;

		//runUdpThread = true;

		while(runUdpThread)
		{
			/*lastTime = time;
			time = stopwatch.ElapsedMilliseconds;
			Debug.Log("pre:  " + (time - lastTime));*/

			try
			{
				IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref ip);

                x = (short) ( (data[1] << 8) | data[0] );
				y = (short) ( (data[3] << 8) | data[2] );
				z = (short) ( (data[5] << 8) | data[4] );

                //Debug.LogError("x: " + x + " y: " + y + " z: " + z);

				/*if(y == lastY)
				{
					Debug.Log("--- SAME ---");
				}
				else
				{
					lastY = y;
				}*/

				if(!isZset)
				{
					firstZ = z;
					isZset = true;
				}

				if(!isYset)
				{
					firstY = y;
					isYset = true;
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
				//serverError = true;
			}

			//Thread.Sleep(10);
			/*lastTime = time;
			time = stopwatch.ElapsedMilliseconds;
			Debug.Log("post: " + (time - lastTime));*/
		}
	}

	public float GetAxis(string axis)
    {
        float inputAxis = 0;

        if (axis == "Horizontal")
        {
            try
            {
                if(Mathf.Abs((z - firstZ) % 360) > neutralAreaHorizontal) inputAxis = -Mathf.Clamp(((z - firstZ) % 360) / inputDivisorVertical, -1.0f, 1.0f); //TODO
                else inputAxis = 0;

                horizontalInputError = false;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                horizontalInputError = true;
            }
        }
        else if (axis == "Vertical")
        {
            try
            {
                inputAxis = Mathf.Clamp(((y - firstY) % 360) / inputDivisorVertical, -1.0f, 1.0f); //TODO
                //Debug.Log(inputAxis);
                verticalInputError = false;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                verticalInputError = true;
            }
        }

        return inputAxis;
    }

    public float getAngleVertical()
    {
    	return (y - firstY) % 360;
    }

	public float getAngleHorizontal()
    {
    	return (z - firstZ) % 360;
    }    

    // used for any kind of errors, like lost connetion to hardware etc.
    public bool HasThrownErrors()
    {
        return false; //verticalInputError || horizontalInputError || serverError;
    }
}
