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

public class FeedbackServer : MonoBehaviour {

    public EventHandler<FeedbackServerEventArgs> FeedbackRequestSuccessful;

    public const int SMELL_LEMON_VAL = 1;
    public const int SMELL_WOODY_VAL = 2;
    public const int SMELL_BERRY_VAL = 3;

    public const string WIND_TAG = "w";
    public const string HEAT_TAG = "h";
    public const string SMELL_TAG = "s";
    public const string VIBRATION_TAG = "v";
    public const string PAUSE_TAG = "p";


    private String _address = "192.168.1.100";
    private String _updateRoute = "/update";
    private String _resetRoute = "/reset";

    public string[] ALL_TAGS = { WIND_TAG, HEAT_TAG, SMELL_TAG, VIBRATION_TAG, PAUSE_TAG };


    private IEnumerator Post(string route, string rawData, Action callback)
    {
        WWWForm form = ConvertRawDataToForm(rawData);
        callback();

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
                callback();
            }
        }
    }

    protected virtual void OnFeedbackRequestSuccessful()
    {
        if (FeedbackRequestSuccessful != null)
            FeedbackRequestSuccessful(this, new FeedbackServerEventArgs { EventInfo = "FeedbackRequest successful" });
    }

    private WWWForm ConvertRawDataToForm(string data)
    {
        WWWForm form = new WWWForm();

        string[] instructions = data.Split(';');

        foreach (string s in instructions)
        {
            foreach (string tag in ALL_TAGS)
            {
                if (s.Contains(tag))
                    form.AddField(tag, s.Replace(tag, ""));
            }
        }

        return form;
    }

    public void PostChange(string rawData, Action callback)
    {
        StartCoroutine(Post("/update", rawData, callback));
    }
}
