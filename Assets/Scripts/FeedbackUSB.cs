using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackUSB : MonoBehaviour {

    public const int SMELL_LEMON_VAL = 3;
    public const int SMELL_WOODY_VAL = 1;
    public const int SMELL_BERRY_VAL = 4;

    public const string WIND_TAG = "w";
    public const string HEAT_TAG = "h";
    public const string SMELL_TAG = "s";
    public const string VIBRATION_TAG = "v";

    public void UpdateFeedback(string rawData, Action callback)
    {

    }
}
