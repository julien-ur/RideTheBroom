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

    private string _address = "192.168.137.102";
    private string _updateRoute = "/update";
    private string _resetRoute = "/reset";

    private string[] ALL_TAGS = { FeedbackConstants.WIND_TAG, FeedbackConstants.HEAT_TAG, FeedbackConstants.SMELL_TAG, FeedbackConstants.VIBRATION_TAG };

    void OnDestroy()
    {
        WWWForm form = new WWWForm();
        form.AddField(FeedbackConstants.WIND_TAG, 0);
        form.AddField(FeedbackConstants.HEAT_TAG, 0);
        WWW www = new WWW(_address + _updateRoute, form);
    }

    private IEnumerator Post(string route, string rawData, Action callback=null)
    {
        string feedbackTag = GetFeedbackTag(rawData);
        WWWForm form = ConvertRawDataToForm(feedbackTag, rawData);
        if (callback != null) callback();

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
                yield return new WaitForSecondsRealtime(FeedbackConstants.SENSE_LATENCY_DICT[feedbackTag]);
                Debug.Log("feedback at player");
                // if (callback != null) callback();
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
            fTag = FeedbackConstants.HEAT_TAG;
        else if (fType == UserStudyControl.FeedbackType.Smell)
            fTag = FeedbackConstants.SMELL_TAG;
        else if (fType == UserStudyControl.FeedbackType.Vibration)
            fTag = FeedbackConstants.VIBRATION_TAG;

        return FeedbackConstants.SENSE_LATENCY_DICT[fTag];
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
