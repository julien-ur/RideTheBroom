using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public int ActionsToSpawn = 5;
    public float MinTimeBetweenActions = 3; //secs

    public EventHandler ActionCountReached;

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
        var rndType = (USAction.TYPE) Random.Range(0, _action.GetTypeCount() - 1);
        _action.StartNewAction(rndType, _actionCount);
        _actionFinished = false;
        _actionCount++;
    }

    private IEnumerator Spawner()
    {
        while (true)
        {
            yield return new WaitUntil(IsPlayerReady);
            
            StartNewAction();
            yield return new WaitUntil(() => _actionFinished);

            if (_actionCount >= ActionsToSpawn)
            {
                yield return new WaitForSeconds(1);
                OnActionCountReached();
                yield break;
            }

            yield return new WaitForSeconds(MinTimeBetweenActions);
        }
    }

    private bool IsPlayerReady()
    {
        return true;
    }

    public void OnActionFinished(object sender, USActionEventArgs args)
    {
        _actionFinished = true;
    }

    protected virtual void OnActionCountReached()
    {
        if (ActionCountReached != null)
            ActionCountReached(this, EventArgs.Empty);
    }
}
