using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackConstants : MonoBehaviour
{

    public const int SMELL_LEMON_VAL = 1;
    public const int SMELL_WOODY_VAL = 2;
    public const int SMELL_BERRY_VAL = 3;

    public const string WIND_TAG = "w";
    public const string HEAT_TAG = "h";
    public const string SMELL_TAG = "s";
    public const string VIBRATION_TAG = "v";

    public static string[] ALL_TAGS = {
        WIND_TAG, HEAT_TAG, SMELL_TAG, VIBRATION_TAG
    };

    public static Dictionary<string, float> SENSE_LATENCY_DICT = new Dictionary<string, float>
    {
        { HEAT_TAG, 0.2f },
        { SMELL_TAG, 0.6f },
        { VIBRATION_TAG, 0.1f }
    };
}
