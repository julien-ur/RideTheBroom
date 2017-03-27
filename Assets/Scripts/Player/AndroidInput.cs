using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class AndroidInput : MonoBehaviour
{
	//public string ipAdress = "192.168.0.232";
	public int port = 5005;

    private bool serverError = false;
    private bool verticalInputError = false;
    private bool horizontalInputError = false;

    public float inputDivisorVertical = 30.0f; // 45
    public float inputDivisorHorizontal = 45.0f; // 60
    public float neutralAreaHorizontal = 0.0f; // 10

    private float firstZ = 0;
    private float firstY = 0;
    private bool isZset = false;
    private bool isYset = false;

    Thread udpThread;
    UdpClient client;

    private short x, y, z;

	void Start ()
	{
		udpThread = new Thread( new ThreadStart(ReceiveDataUDP) );
		udpThread.IsBackground = true;
		udpThread.Start();
	}
	
	void Update ()
	{
		Debug.Log("x: " + x + ", y: " + y + ", z: " + z);
	}

	private void ReceiveDataUDP()
	{
		client = new UdpClient(port);

		while(true)
		{
			try
			{
				IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref ip);

				x = (short) ( (data[1] << 8) | data[0] );
				y = (short) ( (data[3] << 8) | data[2] );
				z = (short) ( (data[5] << 8) | data[4] );

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

				Thread.Sleep(20);
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
				//serverError = true;
			}
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
                Debug.Log(inputAxis);
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
