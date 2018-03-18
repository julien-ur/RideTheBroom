using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class FeedbackServerRequests : MonoBehaviour
{

    public String address = "192.168.1.100";
    public float wind = 5;
    public bool shouldPost;

	void Update ()
	{
	    if (!shouldPost) return;
        StartCoroutine(post());
	    shouldPost = !shouldPost;
	}

    IEnumerator post()
    {
        WWWForm form = new WWWForm();
        form.AddField("wind", wind.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(address + "/update", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}
