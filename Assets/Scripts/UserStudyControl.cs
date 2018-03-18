using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyControl : MonoBehaviour {

    private void Start()
    {
        StartCoroutine(InitStudy());
    }

    private void StartStudy()
    {
        var u = new GameObject()
        {
            name = "User Study"
        };

        var spawner = u.AddComponent<USActionSpawner>();
        var action = u.AddComponent<USAction>();

        action.ActionSuccess += spawner.OnActionSuccess;
        action.ActionTimeOut += spawner.OnActionTimeOut;
    }

    private IEnumerator InitStudy()
    {
        // yield return new WaitUntil(IsPlayerReady);
        yield return new WaitForSeconds(5);
        StartStudy();
    }

    private bool IsPlayerReady()
    {
        return true;
    }
}
