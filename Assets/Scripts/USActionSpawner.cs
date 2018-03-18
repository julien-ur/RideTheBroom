using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public int ActionsToSpawn = 10;
    public float MinTimeBetweenActions = 5; //secs

    private USAction _action;
    private int _actionCount = 0;
    private bool _actionFinished = false;


    public void Start()
    {
        _action = GetComponent<USAction>();
        StartCoroutine(Spawner());
    }

    private void StartNewAction()
    {
        _actionFinished = false;

        var rndType = (USAction.TYPE) Random.Range(0, _action.GetTypeCount() - 1);
        _action.StartNewAction(rndType, _actionCount);

        Debug.Log("Action " + _actionCount + " " + rndType + " started");
        _actionCount++;
    }

    private void NotifyNewAction()
    {
        if (_actionCount >= ActionsToSpawn)
        {
            // notify listener
        }
    }

    private IEnumerator Spawner()
    {
        while (_actionCount < ActionsToSpawn)
        {
            StartNewAction();
            NotifyNewAction();
            
            yield return new WaitUntil(() => _actionFinished);
            yield return new WaitForSeconds(MinTimeBetweenActions);
            yield return new WaitUntil(IsPlayerReady);
        }
    }

    private bool IsPlayerReady()
    {
        return true;
    }

    public void OnActionSuccess(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Success "  + args.Count + " " + args.ActionType);
        _actionFinished = true;
    }

    public void OnActionTimeOut(object sender, USActionEventArgs args)
    {
        Debug.Log("Action TimeOut " + args.Count + " " + args.ActionType);
        _actionFinished = true;
    }
}
