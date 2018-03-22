using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class FeedbackServer : MonoBehaviour {

    public String Address = "192.168.1.100";
    public const string WIND_TAG = "w";
    public const string HEAT_TAG = "h";
    public const string SMELL_TAG = "s";
    public const string VIBRATION_TAG = "v";
    public const string PAUSE_TAG = "p";

    private IEnumerator Post(string route, string rawData)
    {
        WWWForm form = ConvertRawDataToForm(rawData);

        using (UnityWebRequest www = UnityWebRequest.Post(Address + route, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("feedback request successful");
            }
        }
    }

    private WWWForm ConvertRawDataToForm(string data)
    {
        WWWForm form = new WWWForm();

        string[] instructions = data.Split(';');

        foreach (string s in instructions)
        {
            if (s.Contains("s"))
                form.AddField("smell", s.Replace("s", ""));

            else if (s.Contains("h"))
                form.AddField("heat", s.Replace("h", ""));

            else if (s.Contains("v"))
                form.AddField("vibration", s.Replace("v", ""));

            else if (s.Contains(WIND_TAG))
                form.AddField("wind", s.Replace("w", ""));

            else if (s.Contains("p"))
                form.AddField("pause", s.Replace("w", ""));
        }

        return form;
    }

    public void TempFeedbackChange(string rawData)
    {
        StartCoroutine(Post("/temp_update", rawData));
    }

    public void PermanentFeedbackChange(string rawData)
    {
        StartCoroutine(Post("/update", rawData));
    }
}
