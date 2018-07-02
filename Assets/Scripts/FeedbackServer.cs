using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class FeedbackServerEventArgs : EventArgs
{
    public string EventInfo;
}

public class FeedbackServer : MonoBehaviour
{
    public EventHandler<FeedbackServerEventArgs> FeedbackRequestSuccessful;

    public const int SMELL_LEMON_VAL = 3;
    public const int SMELL_WOODY_VAL = 1;
    public const int SMELL_BERRY_VAL = 4;

    public const string WIND_TAG = "w";
    public const string HEAT_TAG = "h";
    public const string SMELL_TAG = "s";
    public const string VIBRATION_TAG = "v";

    public static Dictionary<string, float> SENSE_LATENCY_DICT = new Dictionary<string, float>
    {
        { HEAT_TAG, 0.2f },
        { SMELL_TAG, 0.6f },
        { VIBRATION_TAG, 0.1f }
    };

    private string _address = "192.168.137.100";
    private string _updateRoute = "/update";
    private string _resetRoute = "/reset";

    public string[] ALL_TAGS = { WIND_TAG, HEAT_TAG, SMELL_TAG, VIBRATION_TAG };

    void OnDestroy()
    {
        WWWForm form = new WWWForm();
        form.AddField(WIND_TAG, 0);
        form.AddField(HEAT_TAG, 0);
        WWW www = new WWW(_address + _updateRoute, form);
    }

    private IEnumerator Post(string route, string rawData, Action callback=null)
    {
        string feedbackTag = GetFeedbackTag(rawData);
        WWWForm form = ConvertRawDataToForm(feedbackTag, rawData);
        // if (callback != null) callback();

        using (UnityWebRequest www = UnityWebRequest.Post(_address + _updateRoute, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("feedback request successful");
                OnFeedbackRequestSuccessful();
                yield return new WaitForSecondsRealtime(SENSE_LATENCY_DICT[feedbackTag]);
                Debug.Log("feedback at player");
                if (callback != null) callback();
            }
        }
    }

    protected virtual void OnFeedbackRequestSuccessful()
    {
        if (FeedbackRequestSuccessful != null)
            FeedbackRequestSuccessful(this, new FeedbackServerEventArgs { EventInfo = "FeedbackRequest successful" });
    }

    private string GetFeedbackTag(string d)
    {
        foreach (string tag in ALL_TAGS)
        {
            if (d.Contains(tag))
                return tag;
        }

        return "";
    }

    private WWWForm ConvertRawDataToForm(string feedbackTag, string data)
    {
        WWWForm form = new WWWForm();

        data = data.Replace(feedbackTag, "");

        string[] instructions = data.Split(';');

        foreach (string i in instructions)
        {
            form.AddField(feedbackTag, i);
        }

        return form;
    }

    public float GetLatencyForFeedbackType(UserStudyControl.FeedbackType fType)
    {
        var fTag = "";

        if (fType == UserStudyControl.FeedbackType.Heat)
            fTag = HEAT_TAG;
        else if (fType == UserStudyControl.FeedbackType.Smell)
            fTag = SMELL_TAG;
        else if (fType == UserStudyControl.FeedbackType.Vibration)
            fTag = VIBRATION_TAG;

        return SENSE_LATENCY_DICT[fTag];
    }

    public void PostChange(string rawData, Action callback)
    {
        StartCoroutine(Post("/update", rawData, callback));
    }

    public void Set(string tag, float val)
    {
        string rawData = tag + val;
        StartCoroutine(Post("/update", rawData));
    }
}
