using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public int ActionsToSpawn = 5;
    public float MinTimeBetweenActions = 5; //secs
    public float MaxTimeBetweenActions = 20; //secs

    public EventHandler ActionCountReached;

    private USAction _action;
    private int _actionCount = 0;
    private bool _actionFinished = false;

    void Start()
    {
        _action = GetComponent<USAction>();
    }

    private void StartNewAction()
    {
        _actionFinished = false;
        var rndType = (USAction.TYPE) Random.Range(0, _action.GetTypeCount() - 1);
        _action.StartNewAction(rndType, _actionCount);
        _actionCount++;
    }

    private IEnumerator Spawner()
    {
        yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));

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

            yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));
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

    public void StartSpawning()
    {
        StartCoroutine(Spawner());
    }

    public void ResetActionCount()
    {
        _actionCount = 0;
    }
}
