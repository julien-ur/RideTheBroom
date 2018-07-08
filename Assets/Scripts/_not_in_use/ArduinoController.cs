using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;

public class ArduinoController : MonoBehaviour {

    public float UpdateInterval = 200;

    private SerialPort _stream;
    private PlayerControl _pc;
    private HeatControl _heatControl;

    private float _windPercent = 0;
    private float _heatPercent = 0;
    private float _defaultHeatPercent = 0;

    private float _maxWindHeatDelta = 0.3f;
    private float _minWindForHeat = 0.1f;

    private float _minPlayerSpeed;
    private float _maxPlayerSpeed;

    private float _lastUpdateTime;

    private void Start()
    {
        try
        {
            _stream = new SerialPort("COM3", 9600);
            _stream.ReadTimeout = 50;
            _stream.Open();
            Debug.Log("Connected to Arduino..");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        _pc = GameComponents.GetPlayerControl();
        _heatControl = GameComponents.GetGameController().GetComponent<HeatControl>();

        _minPlayerSpeed = _pc.GetMinSpeed();
        _maxPlayerSpeed = _pc.GetMaxSpeed();
        _lastUpdateTime = Time.time * 1000;
    }

    private void OnDestroy()
    {
        if (_stream != null)
        {
            Send("w0h0");
            _stream.Close();
        }
    }

    private void Update()
    {
        if (!_stream.IsOpen || Time.time * 1000 - _lastUpdateTime < UpdateInterval) return;

        CalcWindStrength();
        CalcHeatStrength();

        Send("w" + _windPercent + "h" + _heatPercent);

        _lastUpdateTime = Time.time * 1000;
    }


    private void Send(string message)
    {
        _stream.Write(message);
        _stream.BaseStream.Flush();
    }


    private void CalcWindStrength()
    {
        _windPercent = (_pc.GetCurrentSpeed() - _minPlayerSpeed) / (_maxPlayerSpeed - _minPlayerSpeed) + 0.2f;
        if (_pc.GetCurrentSpeed() < _minPlayerSpeed)
        {
            _windPercent = (_pc.GetCurrentSpeed() / _minPlayerSpeed) * 0.2f;
        }
        _windPercent = Mathf.Clamp01(_windPercent);
    }

    private void CalcHeatStrength()
    {
        _heatPercent = _heatControl.GetCurrentHeatPercent();

        // Safety mechanism
        if (_windPercent < _minWindForHeat)
        {
            _heatPercent = 0;
        }
        else if (_heatPercent > _windPercent + _maxWindHeatDelta)
        {
            _heatPercent = _windPercent + _maxWindHeatDelta;
        }

        _heatPercent = Mathf.Clamp01(_heatPercent);
    }
}