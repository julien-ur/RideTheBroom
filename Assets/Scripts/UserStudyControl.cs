using System;
using System.Collections;
using UnityEngine;

public class UserStudyControl : MonoBehaviour {

    private void Start()
    {
        StartCoroutine(InitStudy());
    }

    private IEnumerator InitStudy()
    {
        // yield return new WaitUntil(IsPlayerReady);
        yield return new WaitForSeconds(5);
        StartStudy();
        Debug.Log("Study Started");
    }

    private void StartStudy()
    {
        var u = new GameObject()
        {
            name = "User Study"
        };

        var spawner = u.AddComponent<USActionSpawner>();
        var action = u.AddComponent<USAction>();
        var actionControl = u.AddComponent<USActionController>();

        action.ActionStarted += actionControl.OnActionStarted;

        action.ActionSuccess += spawner.OnActionFinished;
        action.ActionSuccess += actionControl.OnActionSuccess;

        action.ActionTimeOut += spawner.OnActionFinished;
        action.ActionTimeOut += actionControl.OnActionTimeOut;

        spawner.ActionCountReached += OnStudyFinished;

        StartLogging();
    }

    private void StartLogging()
    {

    }

    private void FinishLogging()
    {

    }

    private bool IsPlayerReady()
    {
        return true;
    }

    public void OnStudyFinished(object sender, EventArgs args)
    {
        Debug.Log("Study Finished");
        FinishLogging();
    }
}
